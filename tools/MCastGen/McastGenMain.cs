using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MCastGen
{
	/// <summary>
	/// Descripción breve de Form1.
	/// </summary>
	public class MCastGenMain : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label lbAdd;
		private System.Windows.Forms.Label lbPort;
		private System.Windows.Forms.TextBox tbAdd;
		private System.Windows.Forms.TextBox tbPort;
		private System.Windows.Forms.Button btSend;
		private System.Windows.Forms.TextBox tbInterfaz;
		private System.Windows.Forms.Label label1;
        private TextBox tbName;
        private Label label2;
		/// <summary>
		/// Variable del diseñador requerida.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public MCastGenMain()
		{
			//
			// Necesario para admitir el Diseñador de Windows Forms
			//
			InitializeComponent();

			//
			// Todo: agregar código de constructor después de llamar a InitializeComponent
			//
		}

		/// <summary>
		/// Limpiar los recursos que se estén utilizando.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Código generado por el Diseñador de Windows Forms
		/// <summary>
		/// Método necesario para admitir el Diseñador. No se puede modificar
		/// el contenido del método con el editor de código.
		/// </summary>
		private void InitializeComponent()
		{
            this.lbAdd = new System.Windows.Forms.Label();
            this.lbPort = new System.Windows.Forms.Label();
            this.tbAdd = new System.Windows.Forms.TextBox();
            this.tbPort = new System.Windows.Forms.TextBox();
            this.btSend = new System.Windows.Forms.Button();
            this.tbInterfaz = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lbAdd
            // 
            this.lbAdd.Location = new System.Drawing.Point(16, 48);
            this.lbAdd.Name = "lbAdd";
            this.lbAdd.Size = new System.Drawing.Size(160, 20);
            this.lbAdd.TabIndex = 0;
            this.lbAdd.Text = "Grupo Multicast o Direccion IP";
            this.lbAdd.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbPort
            // 
            this.lbPort.Location = new System.Drawing.Point(16, 80);
            this.lbPort.Name = "lbPort";
            this.lbPort.Size = new System.Drawing.Size(100, 20);
            this.lbPort.TabIndex = 1;
            this.lbPort.Text = "Puerto UDP";
            this.lbPort.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbAdd
            // 
            this.tbAdd.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::MCastGen.Properties.Settings.Default, "ipTo", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.tbAdd.Location = new System.Drawing.Point(208, 46);
            this.tbAdd.Name = "tbAdd";
            this.tbAdd.Size = new System.Drawing.Size(104, 20);
            this.tbAdd.TabIndex = 2;
            this.tbAdd.Text = global::MCastGen.Properties.Settings.Default.ipTo;
            this.tbAdd.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbPort
            // 
            this.tbPort.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::MCastGen.Properties.Settings.Default, "udpPort", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.tbPort.Location = new System.Drawing.Point(256, 78);
            this.tbPort.Name = "tbPort";
            this.tbPort.Size = new System.Drawing.Size(56, 20);
            this.tbPort.TabIndex = 3;
            this.tbPort.Text = global::MCastGen.Properties.Settings.Default.udpPort;
            this.tbPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btSend
            // 
            this.btSend.Location = new System.Drawing.Point(208, 152);
            this.btSend.Name = "btSend";
            this.btSend.Size = new System.Drawing.Size(104, 23);
            this.btSend.TabIndex = 4;
            this.btSend.Text = "Enviar";
            this.btSend.Click += new System.EventHandler(this.OnSend);
            // 
            // tbInterfaz
            // 
            this.tbInterfaz.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::MCastGen.Properties.Settings.Default, "ipFrom", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.tbInterfaz.Location = new System.Drawing.Point(208, 14);
            this.tbInterfaz.Name = "tbInterfaz";
            this.tbInterfaz.Size = new System.Drawing.Size(104, 20);
            this.tbInterfaz.TabIndex = 6;
            this.tbInterfaz.Text = global::MCastGen.Properties.Settings.Default.ipFrom;
            this.tbInterfaz.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(16, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 20);
            this.label1.TabIndex = 5;
            this.label1.Text = "Interfaz";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbName
            // 
            this.tbName.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::MCastGen.Properties.Settings.Default, "tifxName", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.tbName.Location = new System.Drawing.Point(212, 113);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(100, 20);
            this.tbName.TabIndex = 8;
            this.tbName.Text = global::MCastGen.Properties.Settings.Default.tifxName;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(16, 112);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 20);
            this.label2.TabIndex = 7;
            this.label2.Text = "Nombre Pasarela";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // MCastGenMain
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(324, 187);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbInterfaz);
            this.Controls.Add(this.tbPort);
            this.Controls.Add(this.tbAdd);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btSend);
            this.Controls.Add(this.lbPort);
            this.Controls.Add(this.lbAdd);
            this.Name = "MCastGenMain";
            this.Text = "Nucleo DF. Generador Mando P/R";
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		/// <summary>
		/// Punto de entrada principal de la aplicación.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new MCastGenMain());
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void OnSend(object sender, System.EventArgs e)
		{
			try
			{
				int Port = int.Parse(tbPort.Text);
				IPAddress ip = IPAddress.Parse(tbAdd.Text);
				IPEndPoint ipTo = new IPEndPoint(ip, Port);
				UdpClientMulticast udp = new UdpClientMulticast(tbInterfaz.Text);
                
                //Byte[] msg = Encoding.ASCII.GetBytes("Is anybody there");
                //udp.Send(msg, msg.Length, ipTo);
                byte[] men = new byte[35];

                men[0] = 0x32;
                men[33] = (byte)'P';
                men[34] = (byte)'R';

                System.Buffer.BlockCopy(ASCIIEncoding.ASCII.GetBytes(tbName.Text), 0, men, 1, tbName.Text.Length);
                udp.Send(men, 35, ipTo);

                Properties.Settings.Default.Save();
			}
			catch (Exception x)
			{
				MessageBox.Show(x.Message);
			}
		}


	}
	/// <summary>
	/// 
	/// </summary>
	public class UdpClientMulticast : UdpClient
	{
		public UdpClientMulticast(int index)
		{
			int optionValue = (int)IPAddress.HostToNetworkOrder(index);
			this.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, optionValue);
		}
		public UdpClientMulticast(string ip)
		{
			Byte [] itf = (IPAddress.Parse(ip)).GetAddressBytes();
			this.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, itf);
		}
	}
	
}
