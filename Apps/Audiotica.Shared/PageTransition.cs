using System;

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

            if (completed != null)
            {
                completed.Invoke();
            }
        }

        public virtual void PlayReverse(Action completed)
        {
            TransitionHelper.Hide(ToPage);
            TransitionHelper.Show(FromPage);

            if (completed != null)
            {
                completed.Invoke();
            }
        }
    }
}