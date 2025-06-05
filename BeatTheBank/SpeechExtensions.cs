// using System.Globalization;
// using CommunityToolkit.Maui.Media;
//
// namespace BeatTheBank;
//
//
// public enum SpeechResult
// {
//     Continue,
//     Stop,
//     Restart
// }
// public static class SpeechExtensions
// {
//     public static IObservable<SpeechResult> ListenUntil(this ISpeechToText speechText, params string[] texts) =>
//         Observable
//             .FromAsync(ct => speechText.StartListenAsync(CultureInfo.CurrentCulture, ct))
//             .Select(_ => Observable.Create<SpeechResult>(ob =>
//             {
//                 var handler = new EventHandler<SpeechToTextRecognitionResultCompletedEventArgs>((_, args) =>
//                 {
//                     var value = args.RecognitionResult.ToLower();
//
//                     switch (value)
//                     {
//                         // case "yes":
//                         // case "next":
//                         // case "keep going":
//                         // case "continue":
//                         // case "go":
//                         //     if (this.Continue.CanExecute(null))
//                         //         this.Continue.Execute(null);
//                         //     break;
//                         //
//                         // case "no":
//                         // case "stop":
//                         //     if (this.Stop.CanExecute(null))
//                         //         this.Stop.Execute(null);
//                         //     break;
//                         //
//                         // case "try again":
//                         // case "start over":
//                         // case "restart":
//                         //     if (this.StartOver.CanExecute(null))
//                         //         this.StartOver.Execute(null);
//                         //     break;
//                 });
//                 speechText.RecognitionResultCompleted += handler;
//                 return async () =>
//                 {
//                     await speechText.StopListenAsync();
//                     speechText.RecognitionResultCompleted -= handler;
//                 };
//             }))
//             .Take(1)
//             .Switch();
// }