namespace Utils.Playable
{
    [System.Flags]
    public enum PlayFlags
    {
        None = 0,
        Manual = 1 << 0, // Le Playable ne se joue que lorsqu'on appelle explicitement la méthode Play()
        OnFootStep = 1 << 1,
        

        // Combos utiles (optionnel)
        All = Manual | OnFootStep
    }

    public interface IPlayable
    {
        public PlayFlags PlayFlags => PlayFlags.Manual; // Par défaut, les Playables sont manuels. Overridez cette propriété pour changer le comportement d'auto-play.
        // Cette méthode sera appelée lorsque l'objet est interagi avec
        void Play();
    }
}