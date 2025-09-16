using CommunityToolkit.Maui.Core;    
using CommunityToolkit.Maui.Views;   
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Mercurio.Driver.ViewModels
{
    [QueryProperty(nameof(ScheduleId), "ScheduleId")]
    public partial class SignatureViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _scheduleId;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSignatureCommand))]
        private ObservableCollection<IDrawingLine> _lines = new();

        [ObservableProperty]
        private bool _isBusy;

        private bool CanSaveSignature => Lines.Any() && !IsBusy;

        [RelayCommand(CanExecute = nameof(CanSaveSignature))]
        private async Task SaveSignature()
        {
            IsBusy = true;
            try
            {               
                var signatureStream = await DrawingView.GetImageStream(Lines, new Size(600, 300), Colors.White);

                using var memoryStream = new MemoryStream();
                await signatureStream.CopyToAsync(memoryStream);
                var signatureBytes = memoryStream.ToArray();
                var signatureBase64 = Convert.ToBase64String(signatureBytes);

                // TODO: Llamar al servicio real para enviar la firma al backend.
                // Reemplaza esto con tu llamada a IScheduleService.
                // var success = await _scheduleService.SaveSignatureAsync(ScheduleId, signatureBase64);

                await Task.Delay(1000); // Simulación de llamada de red
                var success = true;

                if (success)
                {
                    await Shell.Current.GoToAsync("..", new Dictionary<string, object>
                    {
                        { "SignatureSaved", true }
                    });
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "Could not save signature.", "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void ClearSignature()
        {
            Lines.Clear();
        }

        [RelayCommand]
        private async Task Cancel()
        {
            await Shell.Current.GoToAsync("..");
        }
      
        public void ForceLandscape()
        {
#if ANDROID
            var activity = Platform.CurrentActivity;
            if (activity != null)
                activity.RequestedOrientation = Android.Content.PM.ScreenOrientation.Landscape;
#endif
        }

        public void RestoreOrientation()
        {
#if ANDROID
            var activity = Platform.CurrentActivity;
            if (activity != null)
                activity.RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
#endif
        }
    }
}