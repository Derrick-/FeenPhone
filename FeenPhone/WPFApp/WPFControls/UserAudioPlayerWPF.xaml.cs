using FeenPhone.WPFApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace FeenPhone.WPFApp.Controls
{
    /// <summary>
    /// Interaction logic for UserAudioPlayerWPF.xaml
    /// </summary>
    public partial class UserAudioPlayerWPF : UserControl, IDisposable
    {
        public static EventHandler<DependencyPropertyChangedEventArgs> AnyLevelDbChanged;

        public readonly UserAudioPlayer Player;

        public UserAudioPlayerWPF() { }

        internal UserAudioPlayerWPF(Guid userID, AudioOutWPF parent)  : this()
        {
            Player = new UserAudioPlayer(userID, parent);
            DataContext = Player;
            InitializeComponent();

            Player.VisSource.LevelDbChanged += OnLevelDbChanged;
        }

        private void OnLevelDbChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (AnyLevelDbChanged != null)
                AnyLevelDbChanged(sender, e);
        }

        public Guid UserID { get { return Player.UserID; } }

        internal void UIUpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Player.UIUpdateTimer_Elapsed(sender, e);
        }

        public void Stop()
        {
            Player.Stop();
        }

        private void Buffer_Clear_Click(object sender, RoutedEventArgs e)
        {
            Player.Stop();
        }

        public void Dispose()
        {
            Player.VisSource.LevelDbChanged -= OnLevelDbChanged;
            Player.Dispose();
        }

    }
}
