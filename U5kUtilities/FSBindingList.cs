using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Reflection;

namespace Utilities
{
   public abstract class FSBindingItem : IEditableObject
   {
      #region Declarations

      // Member variables
      protected bool m_Editing = false;
      protected Hashtable m_OldValues;

      #endregion

      #region Constructor

      public FSBindingItem()
      {
      }

      #endregion

      #region IEditableObject Members

      public virtual void BeginEdit()
      {
         if (m_Editing == false)
         {
            m_Editing = true;
            m_OldValues = new Hashtable();
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(this);
            foreach (PropertyDescriptor property in properties)
            {
               m_OldValues.Add(property, property.GetValue(this));
            }
         }
      }

      public virtual void CancelEdit()
      {
         if (m_Editing == true)
         {
            // Undo edit changes
            PropertyDescriptor property = null;
            foreach (DictionaryEntry entry in m_OldValues)
            {
               property = (PropertyDescriptor)entry.Key;
               property.SetValue(this, entry.Value);
            }

            // Clean up
            m_OldValues = null;
            m_Editing = false;
         }
      }

      public virtual void EndEdit()
      {
         if (m_Editing == true)
         {
            // Clean up
            m_OldValues = null;
            m_Editing = false;
         }
      }

      #endregion
   }

   class FSPropertyComparer<T> : IComparer<T>
   {
      #region Declarations

      // Member variables
      private PropertyDescriptor m_Property;
      private ListSortDirection m_Direction;

      #endregion

      #region Constructor

      public FSPropertyComparer(PropertyDescriptor property, ListSortDirection direction)
      {
         m_Property = property;
         m_Direction = direction;
      }

      #endregion

      #region Properties

      #endregion

      #region Methods

      private int CompareAscending(object xValue, object yValue)
      {
         int result;

         // If values implement IComparer
         if (xValue is IComparable)
         {
            result = ((IComparable)xValue).CompareTo(yValue);
         }
         // If values don't implement IComparer but are equivalent
         else if (xValue.Equals(yValue))
         {
            result = 0;
         }
         // Values don't implement IComparer and are not equivalent, so compare as string values
         else result = xValue.ToString().CompareTo(yValue.ToString());

         // Return result
         return result;
      }

      private int CompareDescending(object xValue, object yValue)
      {
         // Return result adjusted for ascending or descending sort order ie
         // multiplied by 1 for ascending or -1 for descending
         return CompareAscending(xValue, yValue) * -1;
      }

      private object GetPropertyValue(T value, string property)
      {
         // Get property
         PropertyInfo propertyInfo = value.GetType().GetProperty(property);

         // Return value
         return propertyInfo.GetValue(value, null);
      }

      #endregion

      #region IComparer<T>

      public int Compare(T xWord, T yWord)
      {
         // Get property values
         object xValue = GetPropertyValue(xWord, m_Property.Name);
         object yValue = GetPropertyValue(yWord, m_Property.Name);

         // Determine sort order
         if (m_Direction == ListSortDirection.Ascending)
         {
            return CompareAscending(xValue, yValue);
         }
         else
         {
            return CompareDescending(xValue, yValue);
         }
      }

      public bool Equals(T xWord, T yWord)
      {
         return xWord.Equals(yWord);
      }

      public int GetHashCode(T obj)
      {
         return obj.GetHashCode();
      }

      #endregion
   }
   
   public class FSBindingList<T> : BindingList<T>
	{
		#region Declarations

		#region Sorting Declarations

		private bool p_isSorted;
		private PropertyDescriptor p_sortProperty;
		private ListSortDirection p_sortDirection;

		#endregion

		#endregion

		#region Properties

		#region Sorting Properties

		protected override bool IsSortedCore
		{
			get { return p_isSorted; }
		}

		protected override bool SupportsSortingCore
		{
			get { return true; }
		}

		protected override ListSortDirection SortDirectionCore
		{
			get { return p_sortDirection; }
		}

		protected override PropertyDescriptor SortPropertyCore
		{
			get { return p_sortProperty; }
		}

		#endregion

		#region Searching Properties

		protected override bool SupportsSearchingCore
		{
			get { return true; }
		}

		#endregion

		#endregion

		#region Methods

		#region Sorting Methods

		protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
		{
			// Get list to sort
			// Note: this.Items is a non-sortable ICollection<T>
			List<T> items = this.Items as List<T>;

			// Apply and set the sort, if items to sort
			if (items != null)
			{
				FSPropertyComparer<T> pc = new FSPropertyComparer<T>(property, direction);
				items.Sort(pc);
				p_isSorted = true;
			}
			else
			{
				p_isSorted = false;
			}

			p_sortProperty = property;
			p_sortDirection = direction;

			this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}

		protected override void RemoveSortCore()
		{
			p_isSorted = false;
			this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}

		public void Sort(PropertyDescriptor property, ListSortDirection direction)
		{
			this.ApplySortCore(property, direction);
		}

		#endregion

		#region Searching Methods

		/// <summary>
		/// Searches an FSBindingList for a particular item.
		/// </summary>
		/// <param name="property">The property to search.</param>
		/// <param name="key">The value to find.</param>
		/// <returns>The collection index of the found item.</returns>
		protected override int FindCore(PropertyDescriptor property, object key)
		{
			// Exit if no property specified
			if (property == null) return -1;

			// Get list to search
			List<T> items = this.Items as List<T>;

			// Traverse list for value
			foreach (T item in items)
			{
				// Test column search value
				string value = (string)property.GetValue(item);

				// If value is the search value, return the index of the data item
				if ((string)key == value) return IndexOf(item);
			}
			return -1;
		}

		#endregion

		#region Persistence Methods

		/* These methods are used to serialize a collection to a file, 
         * and to deserialize it from a file. They are intended for use 
         * with standalone files, rather than a database. */

		/* NOTE: BindingList<T> is not serializable, but List<T> is. 
		 * So, we cast the list to (List<T>) for loads and saves. */

		/// <summary>
		/// Loads a collection from a serialized file.
		/// </summary>
		/// <param name="filename">The file path of the file to open.</param>
		public void Load(string filename)
		{
			this.ClearItems();

			if (File.Exists(filename))
			{
				BinaryFormatter formatter = new BinaryFormatter();
				using (FileStream stream = new FileStream(filename, FileMode.Open))
				{
					// Deserialize data list items
					((List<T>)this.Items).AddRange((IEnumerable<T>)formatter.Deserialize(stream));
				}
			}

			// Let bound controls know they should refresh their views
			this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}

		/// <summary>
		/// Saves a collection to a serialized file.
		/// </summary>
		/// <param name="filename">The file path of the file to save.</param>
		public void Save(string filename)
		{
			BinaryFormatter formatter = new BinaryFormatter();
			using (FileStream stream = new FileStream(filename, FileMode.Create))
			{
				// Serialize data list items
				formatter.Serialize(stream, (List<T>)this.Items);
			}
		}

		#endregion

		#endregion
	}
}
