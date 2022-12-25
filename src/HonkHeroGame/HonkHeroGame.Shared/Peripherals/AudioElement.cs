using Microsoft.UI.Xaml;
using System;
using Uno.UI.Runtime.WebAssembly;

namespace HonkHeroGame
{
    [HtmlElement("audio")]
    public sealed class AudioElement : FrameworkElement
    {
        #region Fields
        
        private Action Playback; 

        #endregion

        #region Ctor

        public AudioElement(string source, double volume = 1.0, bool loop = false, Action playback = null)
        {
            var audio = "element.style.display = \"none\"; " +
                "element.controls = false; " +
                $"element.src = \"{source}\"; " +
                $"element.volume = {volume}; " +
                $"element.loop = {loop.ToString().ToLower()}; ";

            this.ExecuteJavascript(audio);

            if (playback is not null)
            {
                Playback = playback;
                this.RegisterHtmlEventHandler("ended", EndedEvent);
            }

#if DEBUG
            Console.WriteLine("source: " + source + " volume: " + volume.ToString() + " loop: " + loop.ToString().ToLower());
#endif
        }

        #endregion

        #region Events

        private void EndedEvent(object sender, EventArgs e)
        {
            Playback?.Invoke();
#if DEBUG
            Console.WriteLine("AUDIO PLAY ENDED");
#endif
        } 

        #endregion

        #region Methods

        public void SetSource(string source)
        {
            this.ExecuteJavascript($"element.src = \"{source}\"; ");
        }

        public void Play()
        {
            this.ExecuteJavascript("element.currentTime = 0; element.play();");
        }

        public void Stop()
        {
            this.ExecuteJavascript("element.pause(); element.currentTime = 0;");
        }

        public void Pause()
        {
            this.ExecuteJavascript("element.pause();");
        }

        public void Resume()
        {
            this.ExecuteJavascript("element.play();");
        }

        public void SetVolume(double volume)
        {
            var audio = $"element.volume = {volume}; ";
            this.ExecuteJavascript(audio);
        } 

        #endregion
    }
}
