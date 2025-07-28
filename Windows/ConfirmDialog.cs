using System.Windows;

public static class ConfirmDialog
{
    /// <summary>
    /// Affiche une boîte de confirmation Oui / Non.
    /// </summary>
    /// <param name="message">Le message à afficher.</param>
    /// <param name="title">Le titre de la fenêtre.</param>
    /// <returns>True si l'utilisateur clique sur Oui, sinon False.</returns>
    public static bool AskUser(string fileName = "", string question = "", string title = "Confirmation")
    {
        MessageBoxResult result = MessageBox.Show(
            fileName + question ,
            title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        );

        return result == MessageBoxResult.Yes;
    }
}
