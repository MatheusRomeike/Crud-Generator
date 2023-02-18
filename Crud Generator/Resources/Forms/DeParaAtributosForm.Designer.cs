namespace Crud_Generator.Resources
{
    partial class DeParaAtributosForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.grid = new System.Windows.Forms.DataGridView();
            this.NomeSQL = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NomeC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.confirmar = new System.Windows.Forms.Button();
            this.cancelar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            // 
            // grid
            // 
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.NomeSQL,
            this.NomeC});
            this.grid.Location = new System.Drawing.Point(12, 12);
            this.grid.Name = "grid";
            this.grid.Size = new System.Drawing.Size(650, 326);
            this.grid.TabIndex = 0;
            this.grid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_CellContentClick);
            // 
            // NomeSQL
            // 
            this.NomeSQL.HeaderText = "Nome SQL";
            this.NomeSQL.MinimumWidth = 15;
            this.NomeSQL.Name = "NomeSQL";
            this.NomeSQL.ReadOnly = true;
            this.NomeSQL.Width = 300;
            // 
            // NomeC
            // 
            this.NomeC.HeaderText = "Nome C#";
            this.NomeC.MinimumWidth = 15;
            this.NomeC.Name = "NomeC";
            this.NomeC.Width = 300;
            // 
            // confirmar
            // 
            this.confirmar.BackColor = System.Drawing.Color.Lime;
            this.confirmar.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.confirmar.ForeColor = System.Drawing.SystemColors.ControlText;
            this.confirmar.Location = new System.Drawing.Point(587, 344);
            this.confirmar.Margin = new System.Windows.Forms.Padding(0);
            this.confirmar.Name = "confirmar";
            this.confirmar.Size = new System.Drawing.Size(75, 32);
            this.confirmar.TabIndex = 1;
            this.confirmar.Text = "Confirmar";
            this.confirmar.UseVisualStyleBackColor = false;
            this.confirmar.Click += new System.EventHandler(this.confirmar_Click);
            // 
            // cancelar
            // 
            this.cancelar.BackColor = System.Drawing.Color.Red;
            this.cancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelar.Location = new System.Drawing.Point(497, 344);
            this.cancelar.Margin = new System.Windows.Forms.Padding(0);
            this.cancelar.Name = "cancelar";
            this.cancelar.Size = new System.Drawing.Size(84, 32);
            this.cancelar.TabIndex = 2;
            this.cancelar.Text = "Cancelar";
            this.cancelar.UseVisualStyleBackColor = false;
            this.cancelar.Click += new System.EventHandler(this.cancelar_Click);
            // 
            // DeParaAtributosForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(674, 379);
            this.Controls.Add(this.cancelar);
            this.Controls.Add(this.confirmar);
            this.Controls.Add(this.grid);
            this.Name = "DeParaAtributosForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "De/Para Atributos";
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button confirmar;
        private System.Windows.Forms.Button cancelar;
        public System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.DataGridViewTextBoxColumn NomeSQL;
        private System.Windows.Forms.DataGridViewTextBoxColumn NomeC;
    }
}