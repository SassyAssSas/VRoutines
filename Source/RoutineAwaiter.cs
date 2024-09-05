using System;
using System.Collections.Generic;

namespace Violoncello.Routines {
   public class RoutineAwaiter {
      private Action _onCompleteCallback;

      internal void Complete() {
         _onCompleteCallback?.Invoke();
      }

      public void Then(Action callback) {
         _onCompleteCallback = callback;
      }
   }

   public class RoutineAwaiter<T> {
      private Action<T> _onCompleteCallback;

      internal void Complete(T result) {
         _onCompleteCallback?.Invoke(result);
      }

      public void Then(Action<T> callback) {
         _onCompleteCallback = callback;
      }
   }
}
