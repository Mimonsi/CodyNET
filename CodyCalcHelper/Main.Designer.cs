namespace CodyCalcHelper;

partial class Main
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
        gbStatus = new System.Windows.Forms.GroupBox();
        btnStatusHex = new System.Windows.Forms.Button();
        txtStatusHex = new System.Windows.Forms.TextBox();
        cbStatusCarry = new System.Windows.Forms.CheckBox();
        cbStatusZero = new System.Windows.Forms.CheckBox();
        cbStatusInterruptDisable = new System.Windows.Forms.CheckBox();
        cbStatusDecimalMode = new System.Windows.Forms.CheckBox();
        cbStatusBreakCommand = new System.Windows.Forms.CheckBox();
        cbStatusOverflow = new System.Windows.Forms.CheckBox();
        cbStatusNegative = new System.Windows.Forms.CheckBox();
        gbNumberConversion = new System.Windows.Forms.GroupBox();
        gbAssembler = new System.Windows.Forms.GroupBox();
        txtNCHex = new System.Windows.Forms.TextBox();
        label1 = new System.Windows.Forms.Label();
        label2 = new System.Windows.Forms.Label();
        txtNCDec = new System.Windows.Forms.TextBox();
        label3 = new System.Windows.Forms.Label();
        txtNCBin = new System.Windows.Forms.TextBox();
        txtAssemblerInput = new System.Windows.Forms.TextBox();
        txtAssemblerOutput = new System.Windows.Forms.TextBox();
        btnAssemblerAssemble = new System.Windows.Forms.Button();
        gbDisassembler = new System.Windows.Forms.GroupBox();
        btnDisassemblerDisassemble = new System.Windows.Forms.Button();
        txtDisassemblerOutput = new System.Windows.Forms.TextBox();
        txtDisassemblerInput = new System.Windows.Forms.TextBox();
        gbStatus.SuspendLayout();
        gbNumberConversion.SuspendLayout();
        gbAssembler.SuspendLayout();
        gbDisassembler.SuspendLayout();
        SuspendLayout();
        // 
        // gbStatus
        // 
        gbStatus.Controls.Add(cbStatusOverflow);
        gbStatus.Controls.Add(cbStatusBreakCommand);
        gbStatus.Controls.Add(cbStatusDecimalMode);
        gbStatus.Controls.Add(cbStatusInterruptDisable);
        gbStatus.Controls.Add(cbStatusZero);
        gbStatus.Controls.Add(cbStatusCarry);
        gbStatus.Controls.Add(btnStatusHex);
        gbStatus.Controls.Add(txtStatusHex);
        gbStatus.Location = new System.Drawing.Point(12, 12);
        gbStatus.Name = "gbStatus";
        gbStatus.Size = new System.Drawing.Size(255, 287);
        gbStatus.TabIndex = 0;
        gbStatus.TabStop = false;
        gbStatus.Text = "Status";
        // 
        // btnStatusHex
        // 
        btnStatusHex.Location = new System.Drawing.Point(171, 24);
        btnStatusHex.Name = "btnStatusHex";
        btnStatusHex.Size = new System.Drawing.Size(75, 25);
        btnStatusHex.TabIndex = 1;
        btnStatusHex.Text = "Status Hex";
        btnStatusHex.UseVisualStyleBackColor = true;
        btnStatusHex.Click += btnStatusHex_Click;
        // 
        // txtStatusHex
        // 
        txtStatusHex.Location = new System.Drawing.Point(6, 24);
        txtStatusHex.Name = "txtStatusHex";
        txtStatusHex.Size = new System.Drawing.Size(159, 25);
        txtStatusHex.TabIndex = 0;
        txtStatusHex.TextChanged += txtStatusHex_TextChanged;
        // 
        // cbStatusCarry
        // 
        cbStatusCarry.Location = new System.Drawing.Point(6, 74);
        cbStatusCarry.Name = "cbStatusCarry";
        cbStatusCarry.Size = new System.Drawing.Size(159, 24);
        cbStatusCarry.TabIndex = 2;
        cbStatusCarry.Text = "Carry";
        cbStatusCarry.UseVisualStyleBackColor = true;
        cbStatusCarry.CheckedChanged += cbStatusCarry_CheckedChanged;
        // 
        // cbStatusZero
        // 
        cbStatusZero.Location = new System.Drawing.Point(6, 104);
        cbStatusZero.Name = "cbStatusZero";
        cbStatusZero.Size = new System.Drawing.Size(159, 24);
        cbStatusZero.TabIndex = 3;
        cbStatusZero.Text = "Zero";
        cbStatusZero.UseVisualStyleBackColor = true;
        cbStatusZero.CheckedChanged += cbStatusZero_CheckedChanged;
        // 
        // cbStatusInterruptDisable
        // 
        cbStatusInterruptDisable.Location = new System.Drawing.Point(6, 134);
        cbStatusInterruptDisable.Name = "cbStatusInterruptDisable";
        cbStatusInterruptDisable.Size = new System.Drawing.Size(159, 24);
        cbStatusInterruptDisable.TabIndex = 4;
        cbStatusInterruptDisable.Text = "Interrupt Disable";
        cbStatusInterruptDisable.UseVisualStyleBackColor = true;
        cbStatusInterruptDisable.CheckedChanged += cbStatusInterruptDisable_CheckedChanged;
        // 
        // cbStatusDecimalMode
        // 
        cbStatusDecimalMode.Location = new System.Drawing.Point(6, 164);
        cbStatusDecimalMode.Name = "cbStatusDecimalMode";
        cbStatusDecimalMode.Size = new System.Drawing.Size(159, 24);
        cbStatusDecimalMode.TabIndex = 5;
        cbStatusDecimalMode.Text = "Decimal Mode";
        cbStatusDecimalMode.UseVisualStyleBackColor = true;
        cbStatusDecimalMode.CheckedChanged += cbStatusDecimalMode_CheckedChanged;
        // 
        // cbStatusBreakCommand
        // 
        cbStatusBreakCommand.Location = new System.Drawing.Point(6, 194);
        cbStatusBreakCommand.Name = "cbStatusBreakCommand";
        cbStatusBreakCommand.Size = new System.Drawing.Size(159, 24);
        cbStatusBreakCommand.TabIndex = 6;
        cbStatusBreakCommand.Text = "Break Command";
        cbStatusBreakCommand.UseVisualStyleBackColor = true;
        cbStatusBreakCommand.CheckedChanged += cbStatusBreakCommand_CheckedChanged;
        // 
        // cbStatusOverflow
        // 
        cbStatusOverflow.Location = new System.Drawing.Point(6, 224);
        cbStatusOverflow.Name = "cbStatusOverflow";
        cbStatusOverflow.Size = new System.Drawing.Size(159, 24);
        cbStatusOverflow.TabIndex = 7;
        cbStatusOverflow.Text = "Overflow";
        cbStatusOverflow.UseVisualStyleBackColor = true;
        cbStatusOverflow.CheckedChanged += cbStatusOverflow_CheckedChanged;
        // 
        // cbStatusNegative
        // 
        cbStatusNegative.Location = new System.Drawing.Point(18, 266);
        cbStatusNegative.Name = "cbStatusNegative";
        cbStatusNegative.Size = new System.Drawing.Size(159, 24);
        cbStatusNegative.TabIndex = 8;
        cbStatusNegative.Text = "Negative";
        cbStatusNegative.UseVisualStyleBackColor = true;
        cbStatusNegative.CheckedChanged += cbStatusNegative_CheckedChanged;
        // 
        // gbNumberConversion
        // 
        gbNumberConversion.Controls.Add(label3);
        gbNumberConversion.Controls.Add(txtNCBin);
        gbNumberConversion.Controls.Add(label2);
        gbNumberConversion.Controls.Add(txtNCDec);
        gbNumberConversion.Controls.Add(label1);
        gbNumberConversion.Controls.Add(txtNCHex);
        gbNumberConversion.Location = new System.Drawing.Point(273, 12);
        gbNumberConversion.Name = "gbNumberConversion";
        gbNumberConversion.Size = new System.Drawing.Size(209, 287);
        gbNumberConversion.TabIndex = 9;
        gbNumberConversion.TabStop = false;
        gbNumberConversion.Text = "Number Conversion";
        // 
        // gbAssembler
        // 
        gbAssembler.Controls.Add(btnAssemblerAssemble);
        gbAssembler.Controls.Add(txtAssemblerOutput);
        gbAssembler.Controls.Add(txtAssemblerInput);
        gbAssembler.Location = new System.Drawing.Point(488, 12);
        gbAssembler.Name = "gbAssembler";
        gbAssembler.Size = new System.Drawing.Size(209, 287);
        gbAssembler.TabIndex = 10;
        gbAssembler.TabStop = false;
        gbAssembler.Text = "Assembler";
        // 
        // txtNCHex
        // 
        txtNCHex.Location = new System.Drawing.Point(65, 24);
        txtNCHex.Name = "txtNCHex";
        txtNCHex.Size = new System.Drawing.Size(138, 25);
        txtNCHex.TabIndex = 8;
        txtNCHex.TextChanged += txtNCHex_TextChanged;
        // 
        // label1
        // 
        label1.Location = new System.Drawing.Point(6, 26);
        label1.Name = "label1";
        label1.Size = new System.Drawing.Size(53, 23);
        label1.TabIndex = 9;
        label1.Text = "Hex:";
        // 
        // label2
        // 
        label2.Location = new System.Drawing.Point(6, 57);
        label2.Name = "label2";
        label2.Size = new System.Drawing.Size(53, 23);
        label2.TabIndex = 11;
        label2.Text = "Dec: ";
        // 
        // txtNCDec
        // 
        txtNCDec.Location = new System.Drawing.Point(65, 55);
        txtNCDec.Name = "txtNCDec";
        txtNCDec.Size = new System.Drawing.Size(138, 25);
        txtNCDec.TabIndex = 10;
        txtNCDec.TextChanged += txtNCDec_TextChanged;
        // 
        // label3
        // 
        label3.Location = new System.Drawing.Point(6, 88);
        label3.Name = "label3";
        label3.Size = new System.Drawing.Size(53, 23);
        label3.TabIndex = 13;
        label3.Text = "Bin:";
        // 
        // txtNCBin
        // 
        txtNCBin.Location = new System.Drawing.Point(6, 114);
        txtNCBin.Name = "txtNCBin";
        txtNCBin.Size = new System.Drawing.Size(197, 25);
        txtNCBin.TabIndex = 12;
        txtNCBin.TextChanged += txtNCBin_TextChanged;
        // 
        // txtAssemblerInput
        // 
        txtAssemblerInput.Location = new System.Drawing.Point(6, 24);
        txtAssemblerInput.Multiline = true;
        txtAssemblerInput.Name = "txtAssemblerInput";
        txtAssemblerInput.Size = new System.Drawing.Size(197, 87);
        txtAssemblerInput.TabIndex = 0;
        // 
        // txtAssemblerOutput
        // 
        txtAssemblerOutput.Location = new System.Drawing.Point(6, 164);
        txtAssemblerOutput.Multiline = true;
        txtAssemblerOutput.Name = "txtAssemblerOutput";
        txtAssemblerOutput.Size = new System.Drawing.Size(197, 114);
        txtAssemblerOutput.TabIndex = 1;
        // 
        // btnAssemblerAssemble
        // 
        btnAssemblerAssemble.Location = new System.Drawing.Point(6, 134);
        btnAssemblerAssemble.Name = "btnAssemblerAssemble";
        btnAssemblerAssemble.Size = new System.Drawing.Size(197, 25);
        btnAssemblerAssemble.TabIndex = 8;
        btnAssemblerAssemble.Text = "Assemble";
        btnAssemblerAssemble.UseVisualStyleBackColor = true;
        btnAssemblerAssemble.Click += btnAssemblerAssemble_Click;
        // 
        // gbDisassembler
        // 
        gbDisassembler.Controls.Add(btnDisassemblerDisassemble);
        gbDisassembler.Controls.Add(txtDisassemblerOutput);
        gbDisassembler.Controls.Add(txtDisassemblerInput);
        gbDisassembler.Location = new System.Drawing.Point(703, 12);
        gbDisassembler.Name = "gbDisassembler";
        gbDisassembler.Size = new System.Drawing.Size(209, 287);
        gbDisassembler.TabIndex = 11;
        gbDisassembler.TabStop = false;
        gbDisassembler.Text = "Disassembler";
        // 
        // btnDisassemblerDisassemble
        // 
        btnDisassemblerDisassemble.Location = new System.Drawing.Point(6, 134);
        btnDisassemblerDisassemble.Name = "btnDisassemblerDisassemble";
        btnDisassemblerDisassemble.Size = new System.Drawing.Size(197, 25);
        btnDisassemblerDisassemble.TabIndex = 8;
        btnDisassemblerDisassemble.Text = "Disassemble";
        btnDisassemblerDisassemble.UseVisualStyleBackColor = true;
        btnDisassemblerDisassemble.Click += btnDisassemblerDisassemble_Click;
        // 
        // txtDisassemblerOutput
        // 
        txtDisassemblerOutput.Location = new System.Drawing.Point(6, 164);
        txtDisassemblerOutput.Multiline = true;
        txtDisassemblerOutput.Name = "txtDisassemblerOutput";
        txtDisassemblerOutput.Size = new System.Drawing.Size(197, 114);
        txtDisassemblerOutput.TabIndex = 1;
        // 
        // txtDisassemblerInput
        // 
        txtDisassemblerInput.Location = new System.Drawing.Point(6, 24);
        txtDisassemblerInput.Multiline = true;
        txtDisassemblerInput.Name = "txtDisassemblerInput";
        txtDisassemblerInput.Size = new System.Drawing.Size(197, 87);
        txtDisassemblerInput.TabIndex = 0;
        // 
        // Main
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(1050, 327);
        Controls.Add(gbDisassembler);
        Controls.Add(gbAssembler);
        Controls.Add(gbNumberConversion);
        Controls.Add(cbStatusNegative);
        Controls.Add(gbStatus);
        Text = "Cody Calc Helper";
        gbStatus.ResumeLayout(false);
        gbStatus.PerformLayout();
        gbNumberConversion.ResumeLayout(false);
        gbNumberConversion.PerformLayout();
        gbAssembler.ResumeLayout(false);
        gbAssembler.PerformLayout();
        gbDisassembler.ResumeLayout(false);
        gbDisassembler.PerformLayout();
        ResumeLayout(false);
    }

    private System.Windows.Forms.TextBox txtAssemblerInput;
    private System.Windows.Forms.TextBox txtAssemblerOutput;
    private System.Windows.Forms.Button btnAssemblerAssemble;
    private System.Windows.Forms.Button btnDisassemblerDisassemble;
    private System.Windows.Forms.TextBox txtDisassemblerInput;

    private System.Windows.Forms.TextBox txtDisassemblerOutput;

    private System.Windows.Forms.GroupBox gbNumberConversion;
    private System.Windows.Forms.GroupBox gbStatus;
    private System.Windows.Forms.GroupBox gbAssembler;
    private System.Windows.Forms.TextBox txtNCHex;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox txtNCDec;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox txtNCBin;

    private System.Windows.Forms.CheckBox cbStatusZero;

    private System.Windows.Forms.CheckBox cbStatusCarry;
    private System.Windows.Forms.CheckBox cbStatusNegative;
    private System.Windows.Forms.CheckBox cbStatusInterruptDisable;
    private System.Windows.Forms.CheckBox cbStatusDecimalMode;
    private System.Windows.Forms.CheckBox cbStatusBreakCommand;
    private System.Windows.Forms.CheckBox cbStatusOverflow;

    private System.Windows.Forms.GroupBox gbDisassembler;
    private System.Windows.Forms.TextBox txtStatusHex;
    private System.Windows.Forms.Button btnStatusHex;

    #endregion
}