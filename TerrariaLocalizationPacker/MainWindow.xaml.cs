using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TerrariaLocalizationPacker.Windows;
using System.IO;
using Path = System.IO.Path;
using File = System.IO.File;
using TerrariaLocalizationPacker.Properties;
using Microsoft.Win32;
using System.Xml;
using System.Diagnostics;
using TerrariaLocalizationPacker.Patching;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using TerrariaLocalizationPacker.Util;

namespace TerrariaLocalizationPacker {
	/**<summary>The main window running Terraria Item Modifier.</summary>*/
	public partial class MainWindow : Window {
		//========== CONSTANTS ===========
		#region Constants

		/**<summary>The possibly paths to the Terraria executable.</summary>*/
		private static readonly string[] PossibleTerrariaPaths = {
			@"C:\Program Files (x86)\Steam\steamapps\common\Terraria\Terraria.exe",
			@"C:\Program Files\Steam\steamapps\common\Terraria\Terraria.exe",
			@"C:\Steam\steamapps\common\Terraria\Terraria.exe"
		};

		#endregion
		//=========== MEMBERS ============
	
		//========= CONSTRUCTORS =========
		#region Constructors

		/**<summary>Constructs the main window.</summary>*/
		public MainWindow() {
			InitializeComponent();

			LoadSettings();
			
			DataObject.AddCopyingHandler(textBoxExe, OnTextBoxCancelDrag);
			
			DataObject.AddPastingHandler(textBoxExe, OnTextBoxQuotesPaste);
		}

		#endregion
		//=========== SETTINGS ===========
		#region Settings

		/**<summary>Loads the application settings.</summary>*/
		private void LoadSettings() {
			LocalizationPacker.ExePath = Settings.Default.ExePath;
			if (string.IsNullOrEmpty(LocalizationPacker.ExePath)) {
				LocalizationPacker.ExePath = "";
				if (!string.IsNullOrEmpty(TerrariaLocator.TerrariaPath)) {
					LocalizationPacker.ExePath = TerrariaLocator.TerrariaPath;
				}
			}
			LocalizationPacker.OutputDirectory = Settings.Default.OutputDirectory;
			LocalizationPacker.InputDirectory = LocalizationPacker.AppDirectory;
			textBoxExe.Text = LocalizationPacker.ExePath;
		}
		/**<summary>Saves the application settings.</summary>*/
		private void SaveSettings() {
			Settings.Default.ExePath = LocalizationPacker.ExePath;
			Settings.Default.Save();
		}

		#endregion
		//=========== HELPERS ============
		#region Helpers
		
		/**<summary>Checks if the path is valid.</summary>*/
		private bool ValidPathTest(bool checkExists = true) {
			if (LocalizationPacker.ExePath == "") {
				TriggerMessageBox.Show(this, MessageIcon.Warning, "Terraria konumu boş olamaz!", "Hatalı Konum");
				return false;
			}
			try {
				if (!File.Exists(LocalizationPacker.ExePath) && checkExists) {
					TriggerMessageBox.Show(this, MessageIcon.Warning, "Terraria uygulaması bulunamadı!", "Uygulama Dosyası Yok");
					return false;
				}
			}
			catch (ArgumentException) {
				TriggerMessageBox.Show(this, MessageIcon.Warning, "Doğru bir Terraria konumu girmen gerekiyor!", "Hatalı Konum");
				return false;
			}
			
			return true;
		}
		/**<summary>Checks if the path is valid.</summary>*/
		private bool ValidPathTest2(bool input) {
			string directory = input ? LocalizationPacker.InputDirectory : LocalizationPacker.OutputDirectory;
			string name = input ? "Repack" : "Unpack";
			if (directory == "") {
				TriggerMessageBox.Show(this, MessageIcon.Warning, "The " + name + " folder path cannot be empty!", "Invalid Path");
				return false;
			}
			try {
				if (!Directory.Exists(directory)) {
					TriggerMessageBox.Show(this, MessageIcon.Warning, "Could not find " + name + " folder!", "Invalid Path");
					return false;
				}
			}
			catch (ArgumentException) {
				TriggerMessageBox.Show(this, MessageIcon.Warning, "You must enter a valid " + name + " folder path!", "Invalid Path");
				return false;
			}

			return true;
		}

		#endregion
		//============ EVENTS ============
		#region Events
		//--------------------------------
		#region Regular

		private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e) {
			SaveSettings();
		}
		private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e) {
			// Make text boxes lose focus on click away
			FocusManager.SetFocusedElement(this, this);
		}
		private void OnTextBoxCancelDrag(object sender, DataObjectCopyingEventArgs e) {
			if (e.IsDragDrop)
				e.CancelCommand();
		}
		private void OnTextBoxQuotesPaste(object sender, DataObjectPastingEventArgs e) {
			var isText = e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true);
			if (!isText) return;

			var text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;
			if (text.StartsWith("\"") || text.EndsWith("\"")) {
				text = text.Trim('"');
				Clipboard.SetText(text);
			}
		}

		#endregion
		//--------------------------------
		#region Packing

		private void OnRepack(object sender, RoutedEventArgs e) {
			MessageBoxResult result;
			if (!ValidPathTest() || !ValidPathTest2(true))
				return;
			File.WriteAllBytes(LocalizationPacker.AppDirectory + "/Terraria.Localization.Content.en-US.json", TerrariaLocalizationPacker.Properties.Resources.Terraria_Localization_Content_en_US);
			File.WriteAllBytes(LocalizationPacker.AppDirectory + "/Terraria.Localization.Content.en-US.Game.json", TerrariaLocalizationPacker.Properties.Resources.Terraria_Localization_Content_en_US_Game);
			File.WriteAllBytes(LocalizationPacker.AppDirectory + "/Terraria.Localization.Content.en-US.Items.json", TerrariaLocalizationPacker.Properties.Resources.Terraria_Localization_Content_en_US_Items);
			File.WriteAllBytes(LocalizationPacker.AppDirectory + "/Terraria.Localization.Content.en-US.Legacy.json", TerrariaLocalizationPacker.Properties.Resources.Terraria_Localization_Content_en_US_Legacy);
			File.WriteAllBytes(LocalizationPacker.AppDirectory + "/Terraria.Localization.Content.en-US.NPCs.json", TerrariaLocalizationPacker.Properties.Resources.Terraria_Localization_Content_en_US_NPCs);
			File.WriteAllBytes(LocalizationPacker.AppDirectory + "/Terraria.Localization.Content.en-US.Projectiles.json", TerrariaLocalizationPacker.Properties.Resources.Terraria_Localization_Content_en_US_Projectiles);
			File.WriteAllBytes(LocalizationPacker.AppDirectory + "/Terraria.Localization.Content.en-US.Town.json", TerrariaLocalizationPacker.Properties.Resources.Terraria_Localization_Content_en_US_Town);
			try {
				bool filesFound = LocalizationPacker.Repack();
				if (filesFound)
                {
					File.Delete(LocalizationPacker.AppDirectory + "/Terraria.Localization.Content.en-US.json");
					File.Delete(LocalizationPacker.AppDirectory + "/Terraria.Localization.Content.en-US.Game.json");
					File.Delete(LocalizationPacker.AppDirectory + "/Terraria.Localization.Content.en-US.Items.json");
					File.Delete(LocalizationPacker.AppDirectory + "/Terraria.Localization.Content.en-US.Legacy.json");
					File.Delete(LocalizationPacker.AppDirectory + "/Terraria.Localization.Content.en-US.NPCs.json");
					File.Delete(LocalizationPacker.AppDirectory + "/Terraria.Localization.Content.en-US.Projectiles.json");
					File.Delete(LocalizationPacker.AppDirectory + "/Terraria.Localization.Content.en-US.Town.json");
					TriggerMessageBox.Show(this, MessageIcon.Info, "Yama başarıyla kuruldu!", "Başarılı");
				}
				else
					TriggerMessageBox.Show(this, MessageIcon.Info, "No localization files with the correct names were found in the Repack folder!", "No Localizations");
			}
			catch (Exception ex) {
				// Automatic Restore:
				bool restored = false; // assigned later, but unused
				bool needsRestore = false;
				if (!File.Exists(LocalizationPacker.ExePath)) {
					needsRestore = true; // File doesn't exist anymore? Who knows, it could happen
				}
				else {
					try {
						// file may still be temporarily in-use, wait a short period of time
						//  (about the same as spent after initial write)
						Thread.Sleep(400);
						// When writing an assembly fails, typically the resulting size is zero bytes.
						// i.e. one time this happens is when resolving references fails.
						// Zero bytes means automatic restore-from-backup
						FileInfo exeInfo = new FileInfo(LocalizationPacker.ExePath);
						needsRestore = (exeInfo.Length == 0);
					}
					catch (Exception) {
						// file may still be in-use... wait a bit longer in the future?
					}
				}
				if (needsRestore && File.Exists(LocalizationPacker.BackupPath)) {
					try {
						LocalizationPacker.Restore();
						restored = true;
					}
					catch (Exception) {
						// No harm done if we wait for the user to restore later.
					}
				}
				result = TriggerMessageBox.Show(this, MessageIcon.Error, "Yama kurulurken bir hata oluştu! Hatayı görmek ister misin?", "Yama Hatası", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes)
					ErrorMessageBox.Show(ex, true);
				return;
			}
		}

		private void OnRestore(object sender, RoutedEventArgs e) {
			MessageBoxResult result;
			if (!ValidPathTest(false))
				return;
			result = TriggerMessageBox.Show(this, MessageIcon.Question, "Yamayı kaldırmak istediğine emin misin?", "Yamayı Kaldır", MessageBoxButton.YesNo);
			if (result == MessageBoxResult.No)
				return;
			if (!File.Exists(LocalizationPacker.BackupPath)) {
				TriggerMessageBox.Show(this, MessageIcon.Warning, "Yedek bulunamadı!", "Kayıp Yedek");
				return;
			}
			try {
				LocalizationPacker.Restore();
				TriggerMessageBox.Show(this, MessageIcon.Info, "Başarıyla kaldırıldı!", "Kaldırıldı");
			}
			catch (Exception ex) {
				result = TriggerMessageBox.Show(this, MessageIcon.Error, "Yama kaldırılırken bir sorun oluştu! Hatayı görmek ister misin?", "Kaldırma Hatası", MessageBoxButton.YesNo);
				if (result == MessageBoxResult.Yes)
					ErrorMessageBox.Show(ex, true);
			}
		}

		#endregion
		//--------------------------------
		#region Settings

		private void OnBrowseExe(object sender, RoutedEventArgs e) {
			OpenFileDialog fileDialog = new OpenFileDialog();

			fileDialog.Title = "Terraria .exesini bul";
			fileDialog.AddExtension = true;
			fileDialog.DefaultExt = ".exe";
			fileDialog.Filter = "Uygulamalar (*.exe)|*.exe|Tüm Dosyalar (*.*)|*.*";
			fileDialog.FilterIndex = 0;
			fileDialog.CheckFileExists = true;
			if (LocalizationPacker.ExePath != "")
				fileDialog.InitialDirectory = LocalizationPacker.ExeDirectory;

			var result = fileDialog.ShowDialog(this);
			if (result.HasValue && result.Value) {
				LocalizationPacker.ExePath = fileDialog.FileName;
				textBoxExe.Text = fileDialog.FileName;
				SaveSettings();
			}
		}
		

		private void OnExeChanged(object sender, TextChangedEventArgs e) {
			LocalizationPacker.ExePath = textBoxExe.Text;
		}
		private void OnOutputChanged(object sender, TextChangedEventArgs e) {
		}
		private void OnInputChanged(object sender, TextChangedEventArgs e) {
		}

		#endregion
		//--------------------------------
		#region Menu Items

		
		private void OnCredits(object sender, RoutedEventArgs e) {
			CreditsWindow.Show(this);
		}


		#endregion
		//--------------------------------
		#endregion
	}
}
