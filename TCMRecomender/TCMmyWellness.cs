using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using CLIPSNET;

namespace TCMmyWellness
{
    public partial class TCMmyWellness : Form
    {
        private class TCMRecomendation
        {
            public string Recomendation { get; set; }
            public int Certainty { get; set; }
        }
        
        
        String[] heartConditions = { "No", "Yes"};
        String[] smokeDrinkConditions = { "No", "Yes" };
        String[] sleepConditions = { "No", "Yes" };
        String[] excerciseConditions = { "No", "Yes" };
        String[] overweightConditions = { "No", "Yes" };
        String[] diabetesConditions = { "No", "Yes" };
        String[] hypertensionConditions = { "No", "Yes" };
        String[] stressConditions = { "No", "Yes" };

        private enum InterviewState { GREETING, INTERVIEW, CONCLUSION };
        List<String> variableAsserts;
        List<String> topAsserts = new List<string>();
        private String lastAnswer = null;
        private String relationAsserted = null;
        private InterviewState interviewState;
        private List<string> priorAnswers = new List<string>();

        private CLIPSNET.Environment clips;
        private bool formLoaded = false;

        public TCMmyWellness()
        {
            InitializeComponent();
            
            heartConditionsComboBox.DataSource = heartConditions;
            smokeDrinkComboBox.DataSource = smokeDrinkConditions;
            sleepComboBox.DataSource = sleepConditions;
            overweightConditionsComboBox.DataSource = overweightConditions;
            diabetesConditionComboBox.DataSource = diabetesConditions;
            hypertensionConditionComboBox.DataSource = hypertensionConditions;
            stressConditionComboBox.DataSource = stressConditions;

            clips = new CLIPSNET.Environment();
            clips.LoadFromResource("TCMmyWellness", "TCMmyWellness.TCMmyWellness.clp");
        }
        
        private void OnChange(object sender, EventArgs e)
        {
            
        }

        private void OnLoad(object sender, EventArgs e)
        {
            RunTCM();
            formLoaded = true;
        }

        private void RunTCM()
        {
            variableAsserts = new List<string>();

            if (formLoaded == true)
            {
                variableAsserts.Add("(attribute (name greeting) (value yes))");
            }
            

            string item = (string)smokeDrinkComboBox.SelectedValue;

            if (item.Equals("Yes"))
            {
                variableAsserts.Add("(attribute (name profile-smoking) (value yes))");
            }

            item = (string)sleepComboBox.SelectedValue;

            if (item.Equals("Yes"))
            {
                variableAsserts.Add("(attribute (name profile-poorsleep) (value yes))");
            }
            
           

            item = (string)overweightConditionsComboBox.SelectedValue;

            if (item.Equals("Yes"))
            {
                variableAsserts.Add("(attribute (name profile-overweight) (value yes))");
            }
           
            item = (string)diabetesConditionComboBox.SelectedValue;

            if (item.Equals("Yes"))
            {
                variableAsserts.Add("(attribute (name profile-diabetes) (value yes))");
            }

            item = (string)hypertensionConditionComboBox.SelectedValue;

            if (item.Equals("Yes"))
            {
                variableAsserts.Add("(attribute (name profile-highblood) (value yes))");
            }

            item = (string)heartConditionsComboBox.SelectedValue;

            if (item.Equals("Yes"))
            {
                variableAsserts.Add("(attribute (name profile-heartdisease) (value yes))");
            }

            item = (string)stressConditionComboBox.SelectedValue;

            if (item.Equals("Yes"))
            {
                variableAsserts.Add("(attribute (name profile-stress) (value yes))");
            }

            item = (string)tongueLabel.Text;

            if (item.Equals("Thin White Coating"))
            {
                variableAsserts.Add("(attribute (name tongue-condition) (value thin-white))");
            }
            else if (item.Equals("Pale With Red Spots"))
            {
                variableAsserts.Add("(attribute (name tongue-condition) (value red-spot))");
            }
            else if (item.Equals("Yellow Greasy Coating"))
            {
                variableAsserts.Add("(attribute (name tongue-condition) (value yellow-greasy))");
            }
            else if (item.Equals("Thin Yellow Coating"))
            {
                variableAsserts.Add("(attribute (name tongue-condition) (value thin-yellow))");
            }
            else if (item.Equals("Thick White Coating"))
            {
                variableAsserts.Add("(attribute (name tongue-condition) (value thick-white))");
            }
            else if (item.Equals("White Greasy Coating"))
            {
                variableAsserts.Add("(attribute (name tongue-condition) (value white-greasy))");
            }
            else if (item.Equals("Cracks And Red Tongue"))
            {
                variableAsserts.Add("(attribute (name tongue-condition) (value cracks))");
            }
            else if (item.Equals("Black Spots And Purple Tongue"))
            {
                variableAsserts.Add("(attribute (name tongue-condition) (value black-purple))");
            }
            else if (item.Equals("Pale Coating"))
            {
                variableAsserts.Add("(attribute (name tongue-condition) (value pale))");
            }
            else
            {
                variableAsserts.Add("(attribute (name tongue-condition) (value null))");
            }

            ProcessRules();

            UpdateTCM();

        }
        
        private void UpdateTCM()
        {
            string evalStr = "(BODY::get-body-list)";

            List<TCMRecomendation> tCMList = new List<TCMRecomendation>();
            int count = 0;

            topAsserts.Clear();

            foreach (FactAddressValue fv in clips.Eval(evalStr) as MultifieldValue)
            {
                if (count < 3)
                {
                    int certainty = (int)((NumberValue)fv["certainty"]);

                    String recomendation = ((LexemeValue)fv["value"]).Value;

                    topAsserts.Add("(top (top " + recomendation + "))");

                    tCMList.Add(new TCMRecomendation() { Recomendation = recomendation, Certainty = certainty });

                    count++;
                }
                
            }
            dataGridView1.DataSource = tCMList;
        }

        private void ProcessRules()
        {
            clips.Reset();

            topAsserts.Reverse();
            foreach (String factString in topAsserts)
            {
                String assertCommand = "(assert " + factString + ")";
                clips.Eval(assertCommand);
            }

            foreach (String factString in variableAsserts)
            {
                String assertCommand = "(assert " + factString + ")";
                clips.Eval(assertCommand);
            }

            clips.Run();
            HandleResponse();
        }

      
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                tongueLabel.Text = item.Text;
            }
        }
        
        private void proceedToQnsButton_Click(object sender, EventArgs e)
        {
            if (formLoaded)
            {
                RunTCM();
                RunTCM();
            }
            
            proceedToQnsButton.Enabled = false;
            heartConditionsComboBox.Enabled = false;
            smokeDrinkComboBox.Enabled = false;
            sleepComboBox.Enabled = false;
            overweightConditionsComboBox.Enabled = false;
            diabetesConditionComboBox.Enabled = false;
            hypertensionConditionComboBox.Enabled = false;
            stressConditionComboBox.Enabled = false;
            listView1.Enabled = false;
            restartButton.Enabled = true;
            //headacheConditionsComboBox.Enabled = false;
        }

        private void OnClickButton(object sender, EventArgs e)
        {
            Button button = sender as Button;

            if (button.Tag.Equals("Next"))
            { NextButtonAction(); }
            else if (button.Tag.Equals("Restart"))
            { NextButtonAction(); }
            else if (button.Tag.Equals("Prev"))
            { PrevButtonAction(); }


            Random rnd = new Random();
            int count = rnd.Next(1, 5);

            //panel1.BackgroundImage;
            //panel1.BackgroundImageLayout = ImageLayout.Stretch;
        }

        private void NextButtonAction()
        {
            String theString ="";
            String theAnswer;

            lastAnswer = null;

            switch (interviewState)
            {
                case InterviewState.GREETING:
                case InterviewState.INTERVIEW:
                    theAnswer = (String)GetCheckedChoiceButton().Tag;
                    theString = "(attribute (name " + relationAsserted + ") (value " + theAnswer + "))";
                    variableAsserts.Add(theString);
                    priorAnswers.Add(theAnswer);
                    break;

                case InterviewState.CONCLUSION:
                    variableAsserts.Clear();
                    priorAnswers.Clear();
                    break;
            }


            ProcessRules();
            UpdateTCM();

        }

        private void PrevButtonAction()
        {
            lastAnswer = priorAnswers.ElementAt(priorAnswers.Count - 1);
            variableAsserts.RemoveAt(variableAsserts.Count - 1);

            if (priorAnswers.Count > 2)
            {
                priorAnswers.RemoveAt(priorAnswers.Count - 1);
            }
            
            ProcessRules();

        }

        private void HandleResponse()
        {
            /*===========================*/
            /* Get the current UI state. */
            /*===========================*/

            String evalStr = "(find-fact ((?f UI-state)) TRUE)";
            FactAddressValue fv = (FactAddressValue)((MultifieldValue)clips.Eval(evalStr))[0];

            /*========================================*/
            /* Determine the Next/Prev button states. */
            /*========================================*/

            if (fv["state"].ToString().Equals("conclusion"))
            {
                interviewState = InterviewState.CONCLUSION;
                //nextButton.Tag = "Restart";
                //nextButton.Text = "Restart";
                nextButton.Visible = false;
                prevButton.Visible = true;
                choicesPanel.Visible = false;
            }
            else if (fv["state"].ToString().Equals("greeting"))
            {
                interviewState = InterviewState.GREETING;
                nextButton.Visible = false;
                //nextButton.Tag = "Next";
                //nextButton.Text = "Next >";
                prevButton.Visible = false;
                choicesPanel.Visible = false;
            }
            else
            {
                interviewState = InterviewState.INTERVIEW;
                nextButton.Visible = true;
                nextButton.Tag = "Next";
                nextButton.Text = "Next >";
                prevButton.Visible = false;
                choicesPanel.Visible = true;
                if (priorAnswers.Count < 1)
                {
                    prevButton.Visible = false;
                }
            }

            /*=====================*/
            /* Set up the choices. */
            /*=====================*/

            choicesPanel.Controls.Clear();

            MultifieldValue damf = (MultifieldValue)fv["display-answers"];
            MultifieldValue vamf = (MultifieldValue)fv["valid-answers"];

            String selected = fv["response"].ToString();
            RadioButton firstButton = null;

            for (int i = 0; i < damf.Count; i++)
            {
                LexemeValue da = (LexemeValue)damf[i];
                LexemeValue va = (LexemeValue)vamf[i];
                RadioButton rButton;
                String buttonName, buttonText, buttonAnswer;

                buttonName = da.Value;
                buttonText = buttonName.Substring(0, 1).ToUpperInvariant() + buttonName.Substring(1);
                buttonAnswer = va.Value;

                rButton = new RadioButton();
                rButton.Text = buttonText;
                if (((lastAnswer != null) && buttonAnswer.Equals(lastAnswer)) ||
                    ((lastAnswer == null) && buttonAnswer.Equals(selected)))
                { rButton.Checked = true; }
                else
                { rButton.Checked = false; }

                rButton.Tag = buttonAnswer;
                rButton.Visible = true;
                rButton.AutoSize = true;
                choicesPanel.Controls.Add(rButton);

                if (firstButton == null)
                { firstButton = rButton; }
            }

            if ((GetCheckedChoiceButton() == null) && (firstButton != null))
            { firstButton.Checked = true; }

            /*====================================*/
            /* Set the label to the display text. */
            /*====================================*/

            relationAsserted = ((LexemeValue)fv["relation-asserted"]).Value;

            /*====================================*/
            /* Set the label to the display text. */
            /*====================================*/

            String messageString = ((StringValue)fv["display"]).Value;

            messageLabel.Text = messageString;
        }

        private RadioButton GetCheckedChoiceButton()
        {
            foreach (RadioButton control in choicesPanel.Controls)
            {
                if (control.Checked)
                { return control; }
            }

            return null;
        }

        private void restartButton_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;
            variableAsserts.Clear();
            topAsserts.Clear();
            ProcessRules();

            panel1.BackgroundImage = Properties.Resources.Homepage;
            listView1.SelectedItems.Clear();
            tongueLabel.Text = "Selected Tongue";

            proceedToQnsButton.Enabled = true;
            heartConditionsComboBox.Enabled = true;
            heartConditionsComboBox.SelectedIndex = 0;
            smokeDrinkComboBox.Enabled = true;
            smokeDrinkComboBox.SelectedIndex = 0;
            sleepComboBox.Enabled = true;
            sleepComboBox.SelectedIndex = 0;
            overweightConditionsComboBox.Enabled = true;
            overweightConditionsComboBox.SelectedIndex = 0;
            diabetesConditionComboBox.Enabled = true;
            diabetesConditionComboBox.SelectedIndex = 0;
            hypertensionConditionComboBox.Enabled = true;
            hypertensionConditionComboBox.SelectedIndex = 0;
            listView1.Enabled = true;
            stressConditionComboBox.Enabled = true;
            stressConditionComboBox.SelectedIndex = 0;
            restartButton.Enabled = false;

        }

        private void messageLabel_TextChanged(object sender, EventArgs e)
        {
           



        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            String recomm = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();

            if (messageLabel.Text == "Please see our conclusion on the right.")
            {


                if (recomm == "qi-stagnation")
                {
                    panel1.BackgroundImage = Properties.Resources.qiS;
                    panel1.BackgroundImageLayout = ImageLayout.Stretch;
                }
                else if (recomm == "qi-deficient")
                {
                    panel1.BackgroundImage = Properties.Resources.qiD;
                    panel1.BackgroundImageLayout = ImageLayout.Stretch;
                }
                else if (recomm == "damp-heat")
                {
                    panel1.BackgroundImage = Properties.Resources.dampH;
                    panel1.BackgroundImageLayout = ImageLayout.Stretch;
                }
                else if (recomm == "heat")
                {
                    panel1.BackgroundImage = Properties.Resources.heat;
                    panel1.BackgroundImageLayout = ImageLayout.Stretch;

                }
                else if (recomm == "yang-deficient")
                {
                    panel1.BackgroundImage = Properties.Resources.yangD;
                    panel1.BackgroundImageLayout = ImageLayout.Stretch;

                }
                else if (recomm == "damp-retention")
                {
                    panel1.BackgroundImage = Properties.Resources.dampR;
                    panel1.BackgroundImageLayout = ImageLayout.Stretch;

                }
                else if (recomm == "yin-deficient")
                {
                    panel1.BackgroundImage = Properties.Resources.yinD;
                    panel1.BackgroundImageLayout = ImageLayout.Stretch;

                }
                else if (recomm == "blood-stasis")
                {
                    panel1.BackgroundImage = Properties.Resources.bloodS;
                    panel1.BackgroundImageLayout = ImageLayout.Stretch;

                }
                else if (recomm == "blood-deficient")
                {
                    panel1.BackgroundImage = Properties.Resources.bloodD;
                    panel1.BackgroundImageLayout = ImageLayout.Stretch;

                }


            }

        }

        private void overweightConditionLabel_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            //tongueLabel.Text = "aaa";
            if (messageLabel.Text == "Please see our conclusion on the right.")
            {

                //topAsserts.Reverse();

                if (topAsserts.FirstOrDefault() == "(top (top qi-stagnation))")
                {
                    panel1.BackgroundImage = Properties.Resources.qiS;
                    panel1.BackgroundImageLayout = ImageLayout.Stretch;
                }
                else if (topAsserts.FirstOrDefault() == "(top (top qi-deficient))")
                {
                    panel1.BackgroundImage = Properties.Resources.qiD;
                    panel1.BackgroundImageLayout = ImageLayout.Stretch;
                }
                else if (topAsserts.FirstOrDefault() == "(top (top damp-heat))")
                {
                    panel1.BackgroundImage = Properties.Resources.dampH;
                    panel1.BackgroundImageLayout = ImageLayout.Stretch;
                }
                else if (topAsserts.FirstOrDefault() == "(top (top heat))")
                {
                    panel1.BackgroundImage = Properties.Resources.heat;
                    panel1.BackgroundImageLayout = ImageLayout.Stretch;

                }
                else if (topAsserts.FirstOrDefault() == "(top (top yang-deficient))")
                {
                    panel1.BackgroundImage = Properties.Resources.yangD;
                    panel1.BackgroundImageLayout = ImageLayout.Stretch;

                }
                else if (topAsserts.FirstOrDefault() == "(top (top damp-retention))")
                {
                    panel1.BackgroundImage = Properties.Resources.dampR;
                    panel1.BackgroundImageLayout = ImageLayout.Stretch;

                }
                else if (topAsserts.FirstOrDefault() == "(top (top yin-deficient))")
                {
                    panel1.BackgroundImage = Properties.Resources.yinD;
                    panel1.BackgroundImageLayout = ImageLayout.Stretch;

                }
                else if (topAsserts.FirstOrDefault() == "(top (top blood-stasis))")
                {
                    panel1.BackgroundImage = Properties.Resources.bloodS;
                    panel1.BackgroundImageLayout = ImageLayout.Stretch;

                }
                else if (topAsserts.FirstOrDefault() == "(top (top blood-deficient))")
                {
                    panel1.BackgroundImage = Properties.Resources.bloodD;
                    panel1.BackgroundImageLayout = ImageLayout.Stretch;

                }
                else
                {
                    panel1.BackgroundImage = Properties.Resources.normal;
                    panel1.BackgroundImageLayout = ImageLayout.Stretch;
                }

            }
            else
            {
                panel1.BackgroundImage = Properties.Resources.Homepage;
                panel1.BackgroundImageLayout = ImageLayout.Stretch;
            }
        }

        private void profileGroupBox_Enter(object sender, EventArgs e)
        {

        }
    }
}
