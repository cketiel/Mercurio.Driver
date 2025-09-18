﻿using CommunityToolkit.Maui.Core;    
using CommunityToolkit.Maui.Views;   
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mercurio.Driver.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Mercurio.Driver.ViewModels
{
    [QueryProperty(nameof(ScheduleId), "ScheduleId")]
    public partial class SignatureViewModel : ObservableObject
    {
        private readonly IScheduleService _scheduleService;

        [ObservableProperty]
        private int _scheduleId;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveSignatureCommand))]
        private ObservableCollection<IDrawingLine> _lines = new();

        [ObservableProperty]
        private bool _isBusy;

        private bool CanSaveSignature => Lines.Any() && !IsBusy;

        public SignatureViewModel(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
            Lines.CollectionChanged += OnLinesCollectionChanged;
        }

        // This method will be executed every time a line is added or removed
        private void OnLinesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // We manually notify the command that its "CanExecute" state may have changed.
            SaveSignatureCommand.NotifyCanExecuteChanged();
        }



        [RelayCommand(CanExecute = nameof(CanSaveSignature))]
        private async Task SaveSignature()
        {
            IsBusy = true;
            try
            {
                // Convert the signature to a stream image
                var signatureStream = await DrawingView.GetImageStream(Lines, new Size(600, 300), Colors.White);

                // Convert stream to byte[]
                using var memoryStream = new MemoryStream();
                await signatureStream.CopyToAsync(memoryStream);
                var signatureBytes = memoryStream.ToArray();

                // Convert the bytes to Base64 to send it through the API
                var signatureBase64 = Convert.ToBase64String(signatureBytes);

                var success = await _scheduleService.SaveSignatureAsync(ScheduleId, signatureBase64);
              
                if (success)
                {
                    // We return to the previous page, passing a parameter to indicate that it was saved
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

#elif IOS    
            // On iOS, this must be configured at the app level or with more complex code.
            // The simplest way is to allow all orientations in Info.plist
            // and let the UI adapt.
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