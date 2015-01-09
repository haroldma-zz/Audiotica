#region

using System;

#endregion

namespace Audiotica
{
    public class PageTransition
    {
        public PageBase FromPage { get; set; }

        public PageBase ToPage { get; set; }

        public virtual void Play(Action completed)
        {
            TransitionHelper.Hide(FromPage);
            TransitionHelper.Show(ToPage);
            completed.Invoke();
        }

        public virtual void PlayReverse(Action completed)
        {
            TransitionHelper.Hide(ToPage);
            TransitionHelper.Show(FromPage);
            completed.Invoke();
        }
    }
}