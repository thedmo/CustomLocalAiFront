namespace LocalAiFront.Components.Pages;

public partial class Home
{
    private bool _seitenleisteOffen;

    private void OeffneSeitenleiste()
    {
        _seitenleisteOffen = true;
    }

    private void SchliesseSeitenleiste()
    {
        _seitenleisteOffen = false;
    }
}
