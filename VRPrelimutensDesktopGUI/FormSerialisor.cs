
/* USAGE
        LOAD

        private void Form1_Load(object sender, EventArgs e)
        {
            FormSerialisor.Deserialise(this, ProgramFilesFolder + @"\client.xml");
            ....
            
            
        SAVE
        
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                FormSerialisor.Serialise(this, ProgramFilesFolder + @"\client.xml");
                button1_Click_1(sender, e);
            }
            catch (Exception ) {
            }
            finally
            {
                Application.Exit();
            };            
        }

*/


using System;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using System.Xml;
using System.Diagnostics;

namespace FormSerialisation {
  public static class FormSerialisor {
    /*
     * Drop this class into your project, and add the following line at the top of any class/form that wishes to use it...
       using FormSerialisation;
       To use the code, simply call FormSerialisor.Serialise(FormOrControlToBeSerialised, FullPathToXMLFile)
     * 
     * For more details, see http://www.codeproject.com/KB/dialog/SavingTheStateOfAForm.aspx
     * 
     * Last updated 13th June '10 to account for the odd behaviour of the two Panel controls in a SplitContainer (see the article)
     */
    public static void Serialise(Control c, string XmlFileName) {
      XmlTextWriter xmlSerialisedForm = new XmlTextWriter(XmlFileName, System.Text.Encoding.Default);
      xmlSerialisedForm.Formatting = Formatting.Indented;
      xmlSerialisedForm.WriteStartDocument();
      xmlSerialisedForm.WriteStartElement("ChildForm");
      // enumerate all controls on the form, and serialise them as appropriate
      AddChildControls(xmlSerialisedForm, c);
      xmlSerialisedForm.WriteEndElement(); // ChildForm
      xmlSerialisedForm.WriteEndDocument();
      xmlSerialisedForm.Flush();
      xmlSerialisedForm.Close();
    }

    private static void AddChildControls(XmlTextWriter xmlSerialisedForm, Control c) {
      foreach (Control childCtrl in c.Controls) {
        if (!(childCtrl is Label)) {
          // serialise this control
          xmlSerialisedForm.WriteStartElement("Control");
          xmlSerialisedForm.WriteAttributeString("Type", childCtrl.GetType().ToString());
          xmlSerialisedForm.WriteAttributeString("Name", childCtrl.Name);
          if (childCtrl is TextBox) {
            xmlSerialisedForm.WriteElementString("Text", ((TextBox)childCtrl).Text);
          } else if (childCtrl is ComboBox) {
            xmlSerialisedForm.WriteElementString("Text", ((ComboBox)childCtrl).Text);
            xmlSerialisedForm.WriteElementString("SelectedIndex", ((ComboBox)childCtrl).SelectedIndex.ToString());
          } else if (childCtrl is ListBox) {
            // need to account for multiply selected items
            ListBox lst = (ListBox)childCtrl;
            if (lst.SelectedIndex == -1) {
              xmlSerialisedForm.WriteElementString("SelectedIndex", "-1");
            } else {
              for (int i = 0; i < lst.SelectedIndices.Count; i++) {
                xmlSerialisedForm.WriteElementString("SelectedIndex", (lst.SelectedIndices[i].ToString()));
              }
            }
            for (int i=0; i < lst.Items.Count; i++)
            {
               xmlSerialisedForm.WriteElementString("Items", (lst.Items[i].ToString()));
            }
          } else if (childCtrl is CheckBox) {
            xmlSerialisedForm.WriteElementString("Checked", ((CheckBox)childCtrl).Checked.ToString());
          } else if (childCtrl is NumericUpDown) {
                        xmlSerialisedForm.WriteElementString("Value", ((NumericUpDown)childCtrl).Value.ToString());
                    }
          // this next line was taken from http://stackoverflow.com/questions/391888/how-to-get-the-real-value-of-the-visible-property
          // which dicusses the problem of child controls claiming to have Visible=false even when they haven't, based on the parent
          // having Visible=true
          bool visible = (bool)typeof(Control).GetMethod("GetState", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(childCtrl, new object[] { 2 });
          xmlSerialisedForm.WriteElementString("Visible", visible.ToString());
          // see if this control has any children, and if so, serialise them
          if (childCtrl.HasChildren) {
            if (childCtrl is SplitContainer) {
              // handle this one as a special case
              AddChildControls(xmlSerialisedForm, ((SplitContainer)childCtrl).Panel1);
              AddChildControls(xmlSerialisedForm, ((SplitContainer)childCtrl).Panel2);
            } else {
              AddChildControls(xmlSerialisedForm, childCtrl);
            }
          }
          xmlSerialisedForm.WriteEndElement(); // Control
        }
      }
    }

    public static void Deserialise(Control c, string XmlFileName) {
      if (File.Exists(XmlFileName)) {
        XmlDocument xmlSerialisedForm = new XmlDocument();
        xmlSerialisedForm.Load(XmlFileName);
        XmlNode topLevel = xmlSerialisedForm.ChildNodes[1];
        foreach (XmlNode n in topLevel.ChildNodes) {
          SetControlProperties((Control)c, n);
        }
      }
    }

    private static void SetControlProperties(Control currentCtrl, XmlNode n) {
      // get the control's name and type
      string controlName = n.Attributes["Name"].Value;
      string controlType = n.Attributes["Type"].Value;
            // find the control
      Control[] ctrl=new Control[0];
      try
      {
                ctrl = currentCtrl.Controls.Find(controlName, true);
      }
      catch (Exception ) { }

      if (ctrl.Length == 0) {
        // can't find the control
      } else {
        Control ctrlToSet = GetImmediateChildControl(ctrl, currentCtrl);
        if (ctrlToSet != null) {
          if (ctrlToSet.GetType().ToString() == controlType) {
            // the right type too ;-)
            switch (controlType) {
              case "System.Windows.Forms.TextBox":
                ((System.Windows.Forms.TextBox)ctrlToSet).Text = n["Text"].InnerText;
                break;
              case "System.Windows.Forms.ComboBox":
                ((System.Windows.Forms.ComboBox)ctrlToSet).Text = n["Text"].InnerText;
                ((System.Windows.Forms.ComboBox)ctrlToSet).SelectedIndex = Convert.ToInt32(n["SelectedIndex"].InnerText);
                break;
              case "System.Windows.Forms.ListBox":
                // need to account for multiply selected items
                ListBox lst = (ListBox)ctrlToSet;
                                
                XmlNodeList xnlSelectedIndex = n.SelectNodes("SelectedIndex");
                for (int i = 0; i < xnlSelectedIndex.Count; i++) {
                  lst.SelectedIndex = Convert.ToInt32(xnlSelectedIndex[i].InnerText);
                }
                lst.Items.Clear();
                XmlNodeList xnlItemsCount = n.SelectNodes("Items");
                for (int i = 0; i < xnlItemsCount.Count; i++)
                {
                    lst.Items.Add(xnlItemsCount[i].InnerText);
                }
                break;
              case "System.Windows.Forms.CheckBox":
                ((System.Windows.Forms.CheckBox)ctrlToSet).Checked = Convert.ToBoolean(n["Checked"].InnerText);
                break;
              case "System.Windows.Forms.NumericUpDown":
                ((System.Windows.Forms.NumericUpDown)ctrlToSet).Value = Convert.ToInt32(n["Value"].InnerText);
                break;
            }                        
            // if n has any children that are controls, deserialise them as well
            if (n.HasChildNodes && ctrlToSet.HasChildren) {
              XmlNodeList xnlControls = n.SelectNodes("Control");
              foreach (XmlNode n2 in xnlControls) {
                SetControlProperties(ctrlToSet, n2);
              }
            }
          } else {
            // not the right type
          }
        } else {
          // can't find a control whose parent is the current control
        }
      }
    }

    private static Control GetImmediateChildControl(Control[] ctrl, Control currentCtrl) {
      Control c = null;
      for (int i = 0; i < ctrl.Length; i++) {
        if ((ctrl[i].Parent.Name == currentCtrl.Name) || (currentCtrl is SplitContainer && ctrl[i].Parent.Parent.Name == currentCtrl.Name)) {
          c = ctrl[i];
          break;
        }
      }
      return c;
    }

  }
}
