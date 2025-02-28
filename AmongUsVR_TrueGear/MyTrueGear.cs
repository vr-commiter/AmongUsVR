using TrueGearSDK;


namespace MyTrueGear
{
    public class TrueGearMod
    {
        private static TrueGearPlayer _player = null;

        public TrueGearMod() 
        {
            _player = new TrueGearPlayer("1849900","Among Us VR");
            _player.Start();
        }

        public void Play(string Event)
        { 
            _player.SendPlay(Event);
        }

    }
}
