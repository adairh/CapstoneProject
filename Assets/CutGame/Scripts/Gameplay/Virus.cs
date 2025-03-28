public class Virus : Destructible
{
    // Executes when virus is killed
    public override void Destroy()
    {
        onDestroy = () => level.Destroy(this);
         
        base.Destroy();


        
        //show panel
        //goto next question


        //in level design, each virus attach 1 question script

    }

    public void onHit()
    {
        GameManager gm = GameManager.instance;
        gm.Quiz.gameObject.SetActive(true);
        gm.SetVirus(this);
    }
    
}
