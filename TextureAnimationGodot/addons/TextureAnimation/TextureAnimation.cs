using Godot;
using System;
using System.Collections;
using System.Threading.Tasks;

[Tool]
public partial class TextureAnimation : TextureRect
{
    private AtlasTexture atlasImage;

    [ExportGroup("GeneralConfigAnimation")]
    [Export] public int timePerMilSeconds = 100; // milisegundos por frame
    [Export] private int totalRows = 1;
    [Export] private int totalCols = 1;
    [Export] private int totalFrames = 1;
    [Export] public int initialFrame { get; private set; } = 0;
    [Export] private int finishFrame = 0;
    [Export] public bool loop = false;
    [Export] private bool autoLoad = false, pausable = true;

    private bool breakAnimation = false, initCorrutine = false;
    private int separationX, separationY;
    public int actualFrame { get; private set; } = 0;
    public bool realActiveAnimation { get; private set; } = false;

    public override void _Ready()
    {
        if (Texture == null || Texture is not AtlasTexture atlas) return;

        atlasImage = (AtlasTexture)Texture;
        separationX = (int)atlasImage.Region.Size.X;
        separationY = (int)atlasImage.Region.Size.Y;
        actualFrame = initialFrame;

        if (autoLoad) _ = TextureAnimRise();
    }

    private void UpdateAtlasFrame(int frame)
    {
        int col = frame % totalCols;
        int row = frame / totalCols;

        int x = separationX * col;
        int y = separationY * row;

        atlasImage.Region = new Rect2(x, y, separationX, separationY);
    }

    private async Task TextureAnimProcess()
    {
        realActiveAnimation = true;
        breakAnimation = false;
        try
        {
            while (!breakAnimation)
            {
                foreach (IEnumerable frame in FrameCalc())
                {
                    while (GetTree().Paused && pausable) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                    await ToSignal(GetTree().CreateTimer(timePerMilSeconds / 1000.0f), SceneTreeTimer.SignalName.Timeout);
                    if (breakAnimation) break;
                }

                if (!loop) break;
            }
        }
        catch (Exception) { }


        realActiveAnimation = false;
    }

    private IEnumerable FrameCalc()
    {
        int startFrame = initialFrame;
        int endFrameRef = (finishFrame == 0) ? totalFrames : finishFrame;
        //bool determinateAvance = (startFrame < finishFrame); in processInverseAnimation

        for (int x = startFrame; x < endFrameRef;)
        {

            actualFrame = x;
            UpdateAtlasFrame(x);
            x++;
            yield return null;
        }
    }


    public async Task TextureAnimRise(Action changeParameters = null, Action changeTime = null)
    {
        if (initCorrutine || atlasImage == null) return;
        initCorrutine = true;

        if (realActiveAnimation)
        {
            breakAnimation = true;
            while (realActiveAnimation) await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
            breakAnimation = false;
        }
        changeParameters?.Invoke();
        changeTime?.Invoke();
        _ = TextureAnimProcess();
        initCorrutine = false;

    }

    public void ChangeAnimation(int frameInitial, int endFrame, int actualframeV, bool loopAnimation)
    {
        initialFrame = frameInitial;
        finishFrame = endFrame;
        actualFrame = actualframeV;
        loop = loopAnimation;
    }

}
