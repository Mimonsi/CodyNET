using CodyNET.Assembler;
using CodyNET.Core.Cody;
using CodyNET.Disassembler;

namespace CodyCalcHelper;

public partial class Main : Form
{
    public Main()
    {
        InitializeComponent();
    }

    private void btnStatusHex_Click(object sender, EventArgs e)
    {
        var text = txtStatusHex.Text;
        
    }

    private bool StatusUpdating = false;
    private void txtStatusHex_TextChanged(object sender, EventArgs e)
    {
        try
        {
            StatusUpdating = true;
            var status = new Status(Convert.ToByte(txtStatusHex.Text, 16));
            cbStatusCarry.Checked = status.Carry;
            cbStatusZero.Checked = status.Zero;
            cbStatusInterruptDisable.Checked = status.InterruptDisable;
            cbStatusDecimalMode.Checked = status.DecimalMode;
            cbStatusBreakCommand.Checked = status.BreakCommand;
            cbStatusOverflow.Checked = status.Overflow;
            cbStatusNegative.Checked = status.Negative;
            btnStatusHex.Text = "OK";
        }
        catch (Exception)
        {
            btnStatusHex.Text = "Err";
        }
        StatusUpdating = false;
    }
    
    private void UpdateStatusFromCheckboxes()
    {
        if (StatusUpdating)
            return;
        var status = new Status()
        {
            Carry = cbStatusCarry.Checked,
            Zero = cbStatusZero.Checked,
            InterruptDisable = cbStatusInterruptDisable.Checked,
            DecimalMode = cbStatusDecimalMode.Checked,
            BreakCommand = cbStatusBreakCommand.Checked,
            Overflow = cbStatusOverflow.Checked,
            Negative = cbStatusNegative.Checked
        };
        txtStatusHex.Text = status.ToByte().ToString("X2");
    }

    private void cbStatusCarry_CheckedChanged(object sender, EventArgs e)
    {
        UpdateStatusFromCheckboxes();
    }

    private void cbStatusInterruptDisable_CheckedChanged(object sender, EventArgs e)
    {
        UpdateStatusFromCheckboxes();
    }

    private void cbStatusZero_CheckedChanged(object sender, EventArgs e)
    {
        UpdateStatusFromCheckboxes();
    }

    private void cbStatusDecimalMode_CheckedChanged(object sender, EventArgs e)
    {
        UpdateStatusFromCheckboxes();
    }

    private void cbStatusBreakCommand_CheckedChanged(object sender, EventArgs e)
    {
        UpdateStatusFromCheckboxes();
    }

    private void cbStatusOverflow_CheckedChanged(object sender, EventArgs e)
    {
        UpdateStatusFromCheckboxes();
    }

    private void cbStatusNegative_CheckedChanged(object sender, EventArgs e)
    {
        UpdateStatusFromCheckboxes();
    }
    
    private bool HexUpdating = false;
    private bool DecUpdating = false;
    private bool BinUpdating = false;
    private void txtNCHex_TextChanged(object sender, EventArgs e)
    {
        if (DecUpdating || BinUpdating)
            return;
        try
        {
            HexUpdating = true;
            txtNCDec.Text = Convert.ToInt32(txtNCHex.Text, 16).ToString();
            txtNCBin.Text = Convert.ToString(Convert.ToInt32(txtNCHex.Text, 16), 2).PadLeft(8, '0');
            HexUpdating = false;
        }
        catch (Exception)
        {
            txtNCDec.Text = "Err";
            txtNCBin.Text = "Err";
        }
    }

    private void txtNCDec_TextChanged(object sender, EventArgs e)
    {
        if (HexUpdating || BinUpdating)
            return;
        try
        {
            DecUpdating = true;
            int value = Convert.ToInt32(txtNCDec.Text);
            txtNCHex.Text = value.ToString("X2");
            txtNCBin.Text = Convert.ToString(value, 2).PadLeft(8, '0');
            DecUpdating = false;
        }
        catch (Exception)
        {
            txtNCHex.Text = "Err";
            txtNCBin.Text = "Err";
        }
    }

    private void txtNCBin_TextChanged(object sender, EventArgs e)
    {
        if (HexUpdating || DecUpdating)
            return;
        try
        {
            BinUpdating = true;
            int value = Convert.ToInt32(txtNCBin.Text, 2);
            txtNCHex.Text = value.ToString("X2");
            txtNCDec.Text = value.ToString();
            BinUpdating = false;
        }
        catch (Exception)
        {
            txtNCHex.Text = "Err";
            txtNCDec.Text = "Err";
        }
    }

    private void btnAssemblerAssemble_Click(object sender, EventArgs e)
    {
        try
        {
            var text = TassAssembler.Assemble(txtAssemblerInput.Text);
            txtAssemblerOutput.Text = string.Join(" ", text.Select(b => b.ToString("X2")));
        }
        catch (Exception x)
        {
            txtAssemblerOutput.Text = x.ToString();
        }
    }

    private void btnDisassemblerDisassemble_Click(object sender, EventArgs e)
    {
        try
        {
            var code = txtDisassemblerInput.Text;
            var byteStrings = code.Split(new[] {' ', '\n', '\r', '\t'}, StringSplitOptions.RemoveEmptyEntries);
            var bytes = new List<byte>();
            foreach (var byteString in byteStrings)
            {
                bytes.Add(Convert.ToByte(byteString, 16));
            }
            var text = CodyDisassembler.Disassemble(bytes.ToArray());
            txtDisassemblerOutput.Text = text;
        }
        catch (Exception x)
        {
            txtDisassemblerOutput.Text = x.ToString();
        }
    }
}