using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using AutoSaveImgClipboard.Helper;
using Tulpep.NotificationWindow;
using UI.FolderSelect;
using System.Resources;
using System.Collections;
using System.Drawing.Imaging;

namespace AutoSaveImgClipboard
{
    public partial class frmConfig : Form
    {
        //Lang
        List<string> strAvailableLang;
        //Global
        string settingtxtPathFolder = null, settingtxtPrefix = null, settingnumericCounter=null;
        int CurrentCounter = -1;//Counter filename
        Dictionary<string, string> dicResources; //Global Resources
        List<string> listHashSavedImages = new List<string>();
        public frmConfig(List<string> localAvailableLang)
        {
            strAvailableLang = localAvailableLang;
            //
            InitializeComponent();
        }
        private void frmConfig_Load(object sender, EventArgs e)
        {
            //Lang UI
            var currentCul=Thread.CurrentThread.CurrentCulture;
            CultureInfo culinfo;
            foreach (var item in strAvailableLang)
            {
                culinfo = new CultureInfo(item);
                Boolean isChecked = false;
                if (currentCul.Name == item) isChecked = true;
                toolButtonLang.DropDownItems.Add(new ToolStripMenuItem(culinfo.NativeName) {Name = culinfo.Name,
                    CheckState = CheckState.Unchecked, Checked=isChecked});
                isChecked = false;
            }
            toolButtonLang.Text = currentCul.DisplayName;
            //read config
            ReadUserData();
            //Set TextControls
            txtPathFolder.Text = settingtxtPathFolder;
            txtPrefix.Text = settingtxtPrefix;
            numericCounter.Text = settingnumericCounter;
            //Start Monitor
            ClipboardMonitor.OnClipboardChange += ClipboardMonitor_OnClipboardChange;
            ClipboardMonitor.Start();
            //Load  Resources
            LoadResources(Thread.CurrentThread.CurrentUICulture.Name);
        }
        private void LoadResources(string language)
        {
            string resxFile = String.Empty;
            // Begin the switch.
            switch (language)
            {
                case "ru-RU":
                    {
                        resxFile = @".\ru\Resource.resx";
                        break;
                    }
                
                default:
                    // You can use the default case.
                    //Load en-US
                    {
                        resxFile = @".\en\Resource.resx";
                    }
                    break;
            }
            //
            ResXResourceReader resxReader = new ResXResourceReader(resxFile);
            dicResources = new Dictionary<string, string>();
            var key = String.Empty; var value = String.Empty;
            foreach (DictionaryEntry entry in resxReader)
            {
                key = (string)entry.Key;
                value= (string)entry.Value;
                dicResources.Add(key, value);
            }
            //If not all resources are translated, 
            //then loading strings from the default language
            if (resxFile!= @".\en\Resource.resx")
            {
                resxFile = @".\en\Resource.resx";
                resxReader = new ResXResourceReader(resxFile);
                key = String.Empty; value = String.Empty;
                foreach (DictionaryEntry entry in resxReader)
                {
                    key = (string)entry.Key;
                    value = (string)entry.Value;
                    //
                    if(!dicResources.ContainsKey(key))
                    {
                        dicResources.Add(key, value);
                    }  
                }
            }
        }
        private void toolButtonLang_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var language = e.ClickedItem.Name;
            var NewCul = new CultureInfo(language);
            //Set New Lang
            var hlpLang = new LanguageChange();
            hlpLang.ChangeLanguage(NewCul);
            hlpLang.ApplyLanguageToForm(this);
            //
            foreach(ToolStripMenuItem menuItem in toolButtonLang.DropDownItems)
            {
                menuItem.Checked = false;
                if(menuItem.Name== e.ClickedItem.Name) menuItem.Checked = true;
            }
            //
            toolButtonLang.Text = NewCul.DisplayName;
            //Load  Resources
            LoadResources(Thread.CurrentThread.CurrentUICulture.Name);
        }
        private void ClipboardMonitor_OnClipboardChange(ClipboardFormat format, object data)
        {
            if (txtPathFolder.Text == String.Empty) return;
            //Save Img
            if (format == ClipboardFormat.Bitmap)
            {
                    //Folder
                    string FolderForImage = txtPathFolder.Text;
                    if (!Directory.Exists(FolderForImage))
                    {
                        Directory.CreateDirectory(FolderForImage);
                    }
                    //FileName
                    if ((CurrentCounter) < 0) CurrentCounter = Convert.ToInt32(numericCounter.Value);
                    //Gen path
                    var PathFile = String.Empty;
                    do
                    {
                        var strCurrentCounter = CurrentCounter.ToString();
                        //Limit Counter
                        if (CurrentCounter > numericCounter.Maximum)
                            {
                                //Warning limit reached
                                //Notifier 
                                var popupNotifier = new PopupNotifier();
                                popupNotifier.TitleText = dicResources["popupNotifierLimitTitleText"];
                                popupNotifier.ContentText = dicResources["popupNotifierLimitContentText"];
                                popupNotifier.Image = SystemIcons.Error.ToBitmap();
                                //
                                popupNotifier.IsRightToLeft = false;
                                popupNotifier.TitleFont = new Font(popupNotifier.TitleFont.FontFamily, 12);
                                popupNotifier.ContentFont = new Font(popupNotifier.ContentFont.FontFamily, 11);
                                popupNotifier.Popup();
                                return;
                            }
                    //
                    for (int i= strCurrentCounter.Length; i< numericCounter.Maximum.ToString().Length;i++)
                        {
                            strCurrentCounter = "0" + strCurrentCounter;
                        }
                        //
                        PathFile = FolderForImage + @"\" + txtPrefix.Text + strCurrentCounter + ".jpg";
                        if (!File.Exists(PathFile)) break;
                        
                        //
                        CurrentCounter++;
                    } while (true);
                    //Save Images
                    var Res = SaveImg(PathFile, data);
                    switch (Res)
                    {
                        case ResultSavingImage.Ok:
                            {
                                //Notifier 
                                var popupNotifier = new PopupNotifier();
                                popupNotifier.TitleText = dicResources["popupNotifierOkTitleText"];
                                popupNotifier.ContentText = dicResources["popupNotifierOkContentText"] + "\n" + PathFile;
                                popupNotifier.Image = SystemIcons.Information.ToBitmap();
                                //
                                popupNotifier.IsRightToLeft = false;
                                popupNotifier.TitleFont = new Font(popupNotifier.TitleFont.FontFamily, 12);
                                popupNotifier.ContentFont = new Font(popupNotifier.ContentFont.FontFamily, 11);
                                popupNotifier.Popup();
                                break;
                            }
                        case ResultSavingImage.Error:
                            {
                                var popupNotifier = new PopupNotifier();
                                popupNotifier.TitleText = dicResources["popupNotifierErrorTitleText"];
                                popupNotifier.ContentText = dicResources["popupNotifierErrorContentText"];
                                popupNotifier.Image = SystemIcons.Error.ToBitmap();
                                //
                                popupNotifier.IsRightToLeft = false;
                                popupNotifier.TitleFont = new Font(popupNotifier.TitleFont.FontFamily, 12);
                                popupNotifier.ContentFont = new Font(popupNotifier.ContentFont.FontFamily, 11);
                                popupNotifier.Popup();
                                break;
                            }
                        case ResultSavingImage.AlreadyExists:
                            {
                                var popupNotifier = new PopupNotifier();
                                popupNotifier.TitleText = dicResources["popupNotifierExistsTitle"];
                                popupNotifier.ContentText = dicResources["popupNotifierExistsContentText"];
                                popupNotifier.Image = SystemIcons.Warning.ToBitmap();
                                //
                                popupNotifier.IsRightToLeft = false;
                                popupNotifier.TitleFont = new Font(popupNotifier.TitleFont.FontFamily, 12);
                                popupNotifier.ContentFont = new Font(popupNotifier.ContentFont.FontFamily, 11);
                                popupNotifier.Popup();
                                break;
                            }
                    }
            }  
        }
        private ResultSavingImage SaveImg(string PathFile, object data)
        {
            //Save to JPG
            Image image;
            image = (Image)data;
            //Get Hash
            string hash = String.Empty;
            hash= ImageHelper.GetSHA1(ImageHelper.ImageToByteArray(image));
            //Check Already
            if (listHashSavedImages.Exists(e => e.EndsWith(hash)))
            {
                return ResultSavingImage.AlreadyExists;
            }
            //
            if (data != null)
            {
                //Get Params for save jpg
                ImageCodecInfo myImageCodecInfo;
                Encoder myEncoder;
                EncoderParameter myEncoderParameter;
                EncoderParameters myEncoderParameters;
                // Get an ImageCodecInfo object that represents the JPEG codec.
                myImageCodecInfo = ImageHelper.GetEncoderInfo("image/jpeg");
                // for the Quality parameter category.
                myEncoder = Encoder.Quality;
                // EncoderParameter object in the array.
                myEncoderParameters = new EncoderParameters(1);
                // Save the bitmap as a JPEG file with quality level 75.
                myEncoderParameter = new EncoderParameter(myEncoder, 95L);
                myEncoderParameters.Param[0] = myEncoderParameter;
                //save
                image.Save(PathFile, myImageCodecInfo, myEncoderParameters);
                //Add in List Saved Hash
                listHashSavedImages.Add(hash);
                return ResultSavingImage.Ok;
            }
            else return ResultSavingImage.Error;
        }
        private void btnHide_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
        }
        private void frmConfig_FormClosed(object sender, FormClosedEventArgs e)
        {
            ClipboardMonitor.Stop();
            WriteUserData();
        }
        private void btnSelect_Click(object sender, EventArgs e)
        {
            var fsd = new FolderSelectDialog();
            fsd.Title = dicResources["SelectFolderTitle"];
            if (settingtxtPathFolder != null)
            {
                fsd.InitialDirectory = settingtxtPathFolder;
            }
            else
            {
                fsd.InitialDirectory = @"c:\";
            }
            //
            if (fsd.ShowDialog(IntPtr.Zero))
            {
                settingtxtPathFolder=txtPathFolder.Text = fsd.FileName;
            }
        }
        private void numericCounter_ValueChanged(object sender, EventArgs e)
        {
            CurrentCounter = Convert.ToInt32(numericCounter.Value);
        }
        private void WriteUserData()
        {
            if (txtPathFolder.Text.Length > 0)
            {
                // create an isolated storage stream...
                FileStream userDataFile =
                  new FileStream("Config.dat", FileMode.Create);
                // create a writer to the stream...
                StreamWriter writeStream = new StreamWriter(userDataFile);
                // write strings to the Isolated Storage file...
                writeStream.WriteLine(txtPathFolder.Text);
                writeStream.WriteLine(txtPrefix.Text);
                writeStream.WriteLine(CurrentCounter.ToString());
                // Tidy up by flushing the stream buffer and then closing
                // the streams...
                writeStream.Flush();
                writeStream.Close();
                userDataFile.Close();
            }
        }
        private void ReadUserData()
        {
            // create an isolated storage stream...
            FileStream userDataFile =
                  new FileStream("Config.dat", FileMode.OpenOrCreate);
            // create a writer to the stream...
            StreamReader readStream = new StreamReader(userDataFile);
            // write strings to the Isolated Storage file...
            settingtxtPathFolder = readStream.ReadLine();
            settingtxtPrefix = readStream.ReadLine();
            settingnumericCounter = readStream.ReadLine();
            // Tidy up by flushing the stream buffer and then closing
            // the streams...
            readStream.Close();
            userDataFile.Close();
        }
        private enum ResultSavingImage
        {
            Ok=0,
            Error=1,
            AlreadyExists=2
        }
    }
}
