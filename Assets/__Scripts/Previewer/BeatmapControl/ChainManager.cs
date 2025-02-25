using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainManager : MonoBehaviour
{
    [Header("Pools")]
    [SerializeField] private ObjectPool chainLinkPool;

    [Header("Object Parents")]
    [SerializeField] private GameObject linkParent;

    [Header("Materials")]
    [SerializeField] private Material complexMaterialRed;
    [SerializeField] private Material complexMaterialBlue;
    [SerializeField] private Material simpleMaterialRed;
    [SerializeField] private Material simpleMaterialBlue;
    [SerializeField] private Material arrowMaterialRed;
    [SerializeField] private Material arrowMaterialBlue;

    public List<Chain> Chains = new List<Chain>();
    public List<ChainLink> ChainLinks = new List<ChainLink>();
    public List<ChainLink> RenderedChainLinks = new List<ChainLink>();

    

    private ObjectManager objectManager;

    public const float Root2Over2 = 0.70711f;

    public static readonly Dictionary<int, Vector2> noteVectorFromDirection = new Dictionary<int, Vector2>
    {
        {0, Vector2.up},
        {1, Vector2.down},
        {2, Vector2.left},
        {3, Vector2.right},
        {4, new Vector2(-Root2Over2, Root2Over2)},
        {5, new Vector2(Root2Over2, Root2Over2)},
        {6, new Vector2(-Root2Over2, -Root2Over2)},
        {7, new Vector2(Root2Over2, -Root2Over2)},
        {8, Vector2.down}
    };


    public void LoadChainsFromDifficulty(Difficulty difficulty)
    {
        ClearRenderedLinks();
        chainLinkPool.SetPoolSize(60);

        Chains.Clear();
        ChainLinks.Clear();

        BeatmapDifficulty beatmap = difficulty.beatmapDifficulty;
        if(beatmap.burstSliders.Length > 0)
        {
            foreach(BurstSlider b in beatmap.burstSliders)
            {
                Chain newChain = Chain.ChainFromBurstSlider(b);
                Chains.Add(newChain);

                CreateChainLinks(newChain);
            }
            Chains = ObjectManager.SortObjectsByBeat<Chain>(Chains);
            ChainLinks = ObjectManager.SortObjectsByBeat<ChainLink>(ChainLinks);
        }
        else
        {
            Chains.Clear();
            ChainLinks.Clear();
        }

        UpdateChainVisuals(TimeManager.CurrentBeat);
    }


    public bool CheckChainHead(Note n)
    {
        List<Chain> chainsOnBeat = ObjectManager.GetObjectsOnBeat<Chain>(Chains, n.Beat);

        for(int i = 0; i < chainsOnBeat.Count; i++)
        {
            Chain c = chainsOnBeat[i];
            if(c.x == n.x && c.y == n.y && c.Color == n.Color)
            {
                return true;
            }
        }

        return false;
    }


    public static Vector2 QuadBezierPoint(Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        return (Mathf.Pow(1 - t, 2) * p0) + (2 * (1 - t) * t * p1) + (Mathf.Pow(t, 2) * p2);
    }


    public static float AngleOnQuadBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        Vector2 derivative = (2 * (1 - t) * (p1 - p0)) + (2 * t * (p2 - p1));
        return 90 + (Mathf.Rad2Deg * Mathf.Atan2(derivative.y, derivative.x));
    }


    private void CreateChainLinks(Chain c)
    {
        Vector2 gridPos = objectManager.bottomLeft;

        Vector2 headPos = new Vector2(c.x * objectManager.laneWidth, c.y * objectManager.rowHeight);
        Vector2 tailPos = new Vector2(c.TailX * objectManager.laneWidth, c.TailY * objectManager.rowHeight);

        //These are the start and end points of the bezier curve
        Vector2 startPos = gridPos + headPos;
        Vector2 endPos = gridPos + tailPos;

        //The midpoint of the curve is 1/2 the distance between the start points, in the direction the chain faces
        float directDistance = Vector2.Distance(startPos, endPos);

        //This is just a failsafe in the case of mapping extensions chains
        //Otherwise an error is thrown because of an out of range dictionary value
        int direction = Mathf.Min(c.Direction, 8);
        Vector2 midOffset = (noteVectorFromDirection[direction] * directDistance) / 2;
        Vector2 midPoint = startPos + midOffset;

        float duration = c.TailBeat - c.Beat;

        //Start at 1 because head note counts as a "segment"
        for(int i = 1; i < c.SegmentCount; i++)
        {
            float timeProgress = (float)i / (c.SegmentCount - 1);

            //Calculate beat based on time progress
            float beat = c.Beat + (duration * timeProgress);

            //Calculate position based on the chain's bezier curve
            float t = timeProgress * c.Squish;
            Vector2 linkPos = QuadBezierPoint(startPos, midPoint, endPos, t);
            float linkAngle = AngleOnQuadBezier(startPos, midPoint, endPos, t) % 360;

            //Check if link isn't taking the shortest path and reverse rotation direction
            if(linkAngle > 180)
            {
                linkAngle -= 360;
            }
            else if(linkAngle < -180)
            {
                linkAngle += 360;
            }

            ChainLink newLink = new ChainLink
            {
                Beat = beat,
                x = linkPos.x,
                y = linkPos.y,
                Color = c.Color,
                Angle = linkAngle
            };
            ChainLinks.Add(newLink);
        }
    }


    public void UpdateLinkVisual(ChainLink cl)
    {
        //Calculate the Z position based on time
        float linkTime = TimeManager.TimeFromBeat(cl.Beat);

        float reactionTime = BeatmapManager.ReactionTime;
        float jumpTime = TimeManager.CurrentTime + reactionTime;

        float worldDist = objectManager.GetZPosition(linkTime);
        Vector3 worldPos = new Vector3(cl.x, cl.y, worldDist);

        if(objectManager.doMovementAnimation)
        {
            float startY = objectManager.objectFloorOffset;
            worldPos.y = objectManager.GetObjectY(startY, worldPos.y, linkTime);
        }

        float angle = cl.Angle;
        float rotationAnimationLength = reactionTime * objectManager.rotationAnimationTime;

        if(objectManager.doRotationAnimation)
        {
            if(linkTime > jumpTime)
            {
                //Note is still jumping in
                angle = 0;
            }
            else if(linkTime > jumpTime - rotationAnimationLength)
            {
                float timeSinceJump = reactionTime - (linkTime - TimeManager.CurrentTime);
                float rotationProgress = timeSinceJump / rotationAnimationLength;
                float angleDist = Easings.Sine.Out(rotationProgress);

                angle *= angleDist;
            }
        }

        if(cl.Visual == null)
        {
            cl.Visual = chainLinkPool.GetObject();
            cl.Visual.transform.SetParent(linkParent.transform);

            cl.chainLinkHandler = cl.Visual.GetComponent<ChainLinkHandler>();
            cl.source = cl.chainLinkHandler.audioSource;

            cl.chainLinkHandler.SetDotMaterial(cl.Color == 0 ? arrowMaterialRed : arrowMaterialBlue);

            if(objectManager.useSimpleNoteMaterial)
            {
                cl.chainLinkHandler.SetMaterial(cl.Color == 0 ? simpleMaterialRed : simpleMaterialBlue);
            }
            else
            {
                cl.chainLinkHandler.SetMaterial(cl.Color == 0 ? complexMaterialRed : complexMaterialBlue);
            }

            cl.Visual.SetActive(true);
            cl.chainLinkHandler.EnableVisual();

            if(TimeManager.Playing && SettingsManager.GetFloat("hitsoundvolume") > 0 && SettingsManager.GetFloat("chainvolume") > 0)
            {
                HitSoundManager.ScheduleHitsound(linkTime, cl.source);
            }

            RenderedChainLinks.Add(cl);
        }

        cl.Visual.transform.localPosition = worldPos;
        cl.Visual.transform.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }


    private void ReleaseChainLink(ChainLink cl)
    {
        cl.source.Stop();
        chainLinkPool.ReleaseObject(cl.Visual);

        cl.Visual = null;
        cl.source = null;
        cl.chainLinkHandler = null;
    }


    public void ClearRenderedLinks()
    {
        if(RenderedChainLinks.Count <= 0)
        {
            return;
        }

        foreach(ChainLink cl in RenderedChainLinks)
        {
            ReleaseChainLink(cl);
        }
        RenderedChainLinks.Clear();
    }


    public void ClearOutsideLinks()
    {
        if(RenderedChainLinks.Count <= 0)
        {
            return;
        }

        for(int i = RenderedChainLinks.Count - 1; i >= 0; i--)
        {
            ChainLink cl = RenderedChainLinks[i];
            if(!objectManager.CheckInSpawnRange(cl.Beat))
            {
                if(cl.source.isPlaying)
                {
                    //Only clear the visual elements if the hitsound is still playing
                    cl.chainLinkHandler.DisableVisual();
                    continue;
                }

                ReleaseChainLink(cl);
                RenderedChainLinks.Remove(cl);
            }
            else if(!cl.chainLinkHandler.Visible)
            {
                cl.chainLinkHandler.EnableVisual();
            }
        }
    }


    public void UpdateChainVisuals(float beat)
    {
        ClearOutsideLinks();

        if(ChainLinks.Count <= 0)
        {
            return;
        }

        int firstLink = ChainLinks.FindIndex(x => objectManager.CheckInSpawnRange(x.Beat));
        if(firstLink >= 0)
        {
            for(int i = firstLink; i < ChainLinks.Count; i++)
            {
                //Update each link's position
                ChainLink cl = ChainLinks[i];
                if(objectManager.CheckInSpawnRange(cl.Beat))
                {
                    UpdateLinkVisual(cl);
                }
                else break;
            }
        }
    }


    public void RescheduleHitsounds(bool playing)
    {
        if(!playing)
        {
            return;
        }

        foreach(ChainLink cl in RenderedChainLinks)
        {
            if(cl.source != null && SettingsManager.GetFloat("hitsoundvolume") > 0 && SettingsManager.GetFloat("chainvolume") > 0)
            {
                HitSoundManager.ScheduleHitsound(TimeManager.TimeFromBeat(cl.Beat), cl.source);
            }
        }
    }


    private void Start()
    {
        objectManager = ObjectManager.Instance;

        TimeManager.OnBeatChanged += UpdateChainVisuals;
        TimeManager.OnPlayingChanged += RescheduleHitsounds;
    }
}