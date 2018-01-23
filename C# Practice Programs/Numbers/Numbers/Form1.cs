using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Numbers
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnIntegers_Click(object sender, EventArgs e)
        {
            int myInteger;
            myInteger = 25;

            MessageBox.Show(myInteger.ToString());
        }

        private void btnFloat_Click(object sender, EventArgs e)
        {
            float myFloat;
            myFloat = 1234.5678F;

            MessageBox.Show(myFloat.ToString());
        }
    }
}
