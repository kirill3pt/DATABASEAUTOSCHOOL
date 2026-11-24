
using System.Windows;

namespace DATABASEAUTOSCHOOL.Interfaces
{
    public partial class InvitationCodeWindow : Window
    {
        public InvitationCodeWindow(string invitationCode)
        {
            InitializeComponent();
            InvitationCodeBox.Text = invitationCode;
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(InvitationCodeBox.Text);

            MessageBox.Show(
                "Код приглашения скопирован.",
                "Приглашение",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
