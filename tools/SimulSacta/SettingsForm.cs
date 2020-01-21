using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using SimulSACTA.Properties;

namespace SimulSACTA
{
   public partial class SettingsForm : Form
   {
      public SettingsForm()
      {
         InitializeComponent();
      }

      private void ConfigForm_Load(object sender, EventArgs e)
      {
         Settings sct = Settings.Default;

         _NetBSendPortTB.Text = sct.ScvPortB.ToString();
         _NetBListenPortTB.Text = sct.ListenPortB.ToString();
         _NetASendPortTB.Text = sct.ScvPortA.ToString();
         _NetAListenPortTB.Text = sct.ListenPortA.ToString();
         _SactaUserGroupTB.Text = sct.SactaGroupUser.ToString();
         _SactaCenterTB.Text = sct.SactaCenter.ToString();
         _SactaDomainTB.Text = sct.SactaDomain.ToString();
         _SPSIUserTB.Text = sct.SactaSPSIUser.ToString();
         _SPVUserTB.Text = sct.SactaSPVUser.ToString();
         _SCVCenterTB.Text = sct.ScvCenter.ToString();
         _SCVDomainTB.Text = sct.ScvDomain.ToString();
         _ProcessorNumberTB.Text = sct.ProcessorNumber.ToString();
         _ActivityTimeoutTB.Text = sct.ActivityTimeOut.ToString();
         _PresenceIntervalTB.Text = sct.PresenceInterval.ToString();



         foreach (string user in sct.ScvUsers)
         {
            _SCVUsersLB.Items.Add(user);
         }
      }

      private void _OkBT_Click(object sender, EventArgs e)
      {
         Settings sct = Settings.Default;

         sct.ScvPortB = Int32.Parse(_NetBSendPortTB.Text);
         sct.ListenPortB = Int32.Parse(_NetBListenPortTB.Text);
         sct.ScvPortA = Int32.Parse(_NetASendPortTB.Text);
         sct.ListenPortA = Int32.Parse(_NetAListenPortTB.Text);
         sct.SactaGroupUser = UInt16.Parse(_SactaUserGroupTB.Text);
         sct.SactaCenter = Byte.Parse(_SactaCenterTB.Text);
         sct.SactaDomain = Byte.Parse(_SactaDomainTB.Text);
         sct.SactaSPVUser = UInt16.Parse(_SPVUserTB.Text);
         sct.SactaSPSIUser = UInt16.Parse(_SPSIUserTB.Text);
         sct.ScvCenter = Byte.Parse(_SCVCenterTB.Text);
         sct.ScvDomain = Byte.Parse(_SCVDomainTB.Text);
         sct.ProcessorNumber = UInt16.Parse(_ProcessorNumberTB.Text);
         sct.ActivityTimeOut = UInt16.Parse(_ActivityTimeoutTB.Text);
         sct.PresenceInterval = UInt16.Parse(_PresenceIntervalTB.Text);

         sct.ScvUsers.Clear();
         for (int i = 0, total = _SCVUsersLB.Items.Count; i < total; i++)
         {
            sct.ScvUsers.Add(_SCVUsersLB.Items[i].ToString());
         }

         sct.Save();
      }

      private void AddSCVUser_Click(object sender, EventArgs e)
      {
         EditBox dlg = new EditBox();
         if (dlg.ShowDialog(this) == DialogResult.OK)
         {
            _SCVUsersLB.Items.Add(dlg.ValueTB.Text);
         }
      }

      private void DeleteSCVUser_Click(object sender, EventArgs e)
      {
         if (_SCVUsersLB.SelectedIndex >= 0)
         {
            _SCVUsersLB.Items.RemoveAt(_SCVUsersLB.SelectedIndex);
         }
      }
   }
}