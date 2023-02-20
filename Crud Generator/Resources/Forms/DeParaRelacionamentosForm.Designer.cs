namespace Crud_Generator.Resources.Forms
{
    partial class DeParaRelacionamentosForm
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
            this.cancelar = new System.Windows.Forms.Button();
            this.confirmar = new System.Windows.Forms.Button();
            this.Atributo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Tabela = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Cardinalidade = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.Classe = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            // 
            // grid
            // 
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Atributo,
            this.Tabela,
            this.Cardinalidade,
            this.Classe});
            this.grid.Location = new System.Drawing.Point(12, 12);
            this.grid.Name = "grid";
            this.grid.Size = new System.Drawing.Size(685, 309);
            this.grid.TabIndex = 0;
            // 
            // cancelar
            // 
            this.cancelar.BackColor = System.Drawing.Color.Red;
            this.cancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelar.Location = new System.Drawing.Point(532, 337);
            this.cancelar.Margin = new System.Windows.Forms.Padding(0);
            this.cancelar.Name = "cancelar";
            this.cancelar.Size = new System.Drawing.Size(84, 32);
            this.cancelar.TabIndex = 4;
            this.cancelar.Text = "Cancelar";
            this.cancelar.UseVisualStyleBackColor = false;
            this.cancelar.Click += new System.EventHandler(this.cancelar_Click);
            // 
            // confirmar
            // 
            this.confirmar.BackColor = System.Drawing.Color.Lime;
            this.confirmar.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.confirmar.ForeColor = System.Drawing.SystemColors.ControlText;
            this.confirmar.Location = new System.Drawing.Point(622, 337);
            this.confirmar.Margin = new System.Windows.Forms.Padding(0);
            this.confirmar.Name = "confirmar";
            this.confirmar.Size = new System.Drawing.Size(75, 32);
            this.confirmar.TabIndex = 3;
            this.confirmar.Text = "Confirmar";
            this.confirmar.UseVisualStyleBackColor = false;
            this.confirmar.Click += new System.EventHandler(this.confirmar_Click);
            // 
            // Atributo
            // 
            this.Atributo.HeaderText = "Atributo";
            this.Atributo.Name = "Atributo";
            this.Atributo.ReadOnly = true;
            this.Atributo.Width = 160;
            // 
            // Tabela
            // 
            this.Tabela.HeaderText = "Tabela";
            this.Tabela.Name = "Tabela";
            this.Tabela.ReadOnly = true;
            this.Tabela.Width = 160;
            // 
            // Cardinalidade
            // 
            this.Cardinalidade.HeaderText = "Cardinalidade (pai x filho)";
            this.Cardinalidade.Items.AddRange(new object[] {
            "1-1",
            "1-n",
            "n-1",
            "n-n"});
            this.Cardinalidade.Name = "Cardinalidade";
            this.Cardinalidade.Width = 160;
            // 
            // Classe
            // 
            this.Classe.HeaderText = "Classe";
            this.Classe.Name = "Classe";
            this.Classe.Width = 160;
            // 
            // DeParaRelacionamentosForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(709, 378);
            this.Controls.Add(this.cancelar);
            this.Controls.Add(this.confirmar);
            this.Controls.Add(this.grid);
            this.Name = "DeParaRelacionamentosForm";
            this.Text = "DeParaRelacionamentos";
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button cancelar;
        private System.Windows.Forms.Button confirmar;
        public System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.DataGridViewTextBoxColumn Atributo;
        private System.Windows.Forms.DataGridViewTextBoxColumn Tabela;
        private System.Windows.Forms.DataGridViewComboBoxColumn Cardinalidade;
        private System.Windows.Forms.DataGridViewTextBoxColumn Classe;
    }
}