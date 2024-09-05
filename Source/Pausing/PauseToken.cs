using System;
using UnityEngine;

namespace Violoncello.Routines {
   public struct PauseToken {
      public readonly bool Paused => IsPaused();
      public readonly bool Resumed => IsResumed();

      private PauseTokenSource _pts;

      public PauseToken(PauseTokenSource pts) {
         _pts = pts;
      }
         
      public readonly bool IsPaused() {
         if (_pts == null) {
            return false;
         }

         return _pts.Paused;
      }

      public readonly bool IsResumed() {
         if (_pts == null) {
            return true;
         }

         return !_pts.Paused;
      }

      internal void Dispose() {
         _pts = null;
      }
   }
}
