using UnityEngine.UI;

public class TestDisplayDynamicSize : TestDisplay
{
    public Image image;
    public Image image2;
    public bool dynamicSizeSet = false;
    public override void Set(string value)
    {
        base.Set(value);
        image.gameObject.SetActive(true);
        image2.gameObject.SetActive(true);
    }
}
