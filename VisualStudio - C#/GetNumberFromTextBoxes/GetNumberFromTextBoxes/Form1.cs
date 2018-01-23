using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetNumberFromTextBoxes
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnAnswer_Click(object sender, EventArgs e)
        {
            int firstTextBoxNumber;
            int answer;
            int secondTextBoxNumber;
            int thirdTextBoxNumber;

            firstTextBoxNumber = int.Parse(tbFirstNumber.Text);
            //firstTextBoxNumber = int.Parse("1845");
            secondTextBoxNumber = int.Parse(tbSecondNumber.Text);
            //secondTexBoxNumber = int.Parse("2858");
            thirdTextBoxNumber = int.Parse(tbthirdNumber.Text);

            answer = (firstTextBoxNumber / secondTextBoxNumber) + thirdTextBoxNumber;            

            //MessageBox.Show(firstTextBoxNumber.ToString());
            MessageBox.Show(answer.ToString());





        }
    }
}
