using System;

namespace Violoncello.Routines {
   public class PauseTokenSource : IDisposable {
      public PauseToken Token { get; private set; }

      public bool Paused { get; private set; }

      public PauseTokenSource() {
         Token = new(this);
      }

      public void Pause() {
         Paused = true;
      }

      public void Resume() {
         Paused = false;
      }

      public void Dispose() {
         Token.Dispose();
      }
   }
}
