using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatmapUtility
{
    public static BeatmapDifficulty AddNullsDifficulty(BeatmapDifficulty diff)
    {
        //Painful method of avoiding nullrefs because json utility is DUMB!!!!HTS!N>H
        if(diff.version == null)
        {
            diff.version = "3.0.0";
        }
        if(diff.bpmEvents == null)
        {
            diff.bpmEvents = new BpmEvent[0];
        }
        if(diff.rotationEvents == null)
        {
            diff.rotationEvents = new RotationEvent[0];
        }
        if(diff.colorNotes == null)
        {
            diff.colorNotes = new ColorNote[0];
        }
        if(diff.bombNotes == null)
        {
            diff.bombNotes = new BombNote[0];
        }
        if(diff.obstacles == null)
        {
            diff.obstacles = new Obstacle[0];
        }
        if(diff.sliders == null)
        {
            diff.sliders = new ArcSlider[0];
        }
        if(diff.burstSliders == null)
        {
            diff.burstSliders = new BurstSlider[0];
        }
        if(diff.basicBeatMapEvents == null)
        {
            diff.basicBeatMapEvents = new BasicBeatmapEvent[0];
        }
        if(diff.colorBoostBeatMapEvents == null)
        {
            diff.colorBoostBeatMapEvents = new ColorBoostBeatmapEvent[0];
        }

        return diff;
    }


    public static BeatmapInfo AddNullsInfo(BeatmapInfo info)
    {
        //This is probably incredibly stupid to do, and there's definitely a better solution
        if(info._songName == null)
        {
            info._songName = "Unknow";
        }
        if(info._songSubName == null)
        {
            info._songSubName = "";
        }
        if(info._songAuthorName == null)
        {
            info._songAuthorName = "Unknow";
        }
        if(info._levelAuthorName == null)
        {
            info._levelAuthorName = "Unknow";
        }
        if(info._songFilename == null)
        {
            info._songFilename = "";
        }
        if(info._coverImageFilename == null)
        {
            info._coverImageFilename = "";
        }
        if(info._environmentName == null)
        {
            info._environmentName = "DefaultEnvironment";
        }
        if(info._allDirectionsEnvironmentName == null)
        {
            info._allDirectionsEnvironmentName = "GlassDesertEnvironment";
        }
        if(info._difficultyBeatmapSets == null)
        {
            info._difficultyBeatmapSets = new DifficultyBeatmapSet[0];
        }
        else
        {
            for(int i = 0; i < info._difficultyBeatmapSets.Length; i++)
            {
                DifficultyBeatmapSet set = info._difficultyBeatmapSets[i];

                if(set._beatmapCharacteristicName == null)
                {
                    set._beatmapCharacteristicName = "Standard";
                }
                if(set._difficultyBeatmaps == null)
                {
                    set._difficultyBeatmaps = new DifficultyBeatmap[0];
                }
            }
        }

        return info;
    }


    public static BeatmapDifficultyV2 AddNullsDifficultyV2(BeatmapDifficultyV2 diff)
    {
        if(diff._version == null)
        {
            diff._version = "2.6.0";
        }
        if(diff._notes == null)
        {
            diff._notes = new NoteV2[0];
        }
        if(diff._sliders == null)
        {
            diff._sliders = new SliderV2[0];
        }
        if(diff._obstacles == null)
        {
            diff._obstacles = new ObstacleV2[0];
        }
        if(diff._events == null)
        {
            diff._events = new EventV2[0];
        }

        return diff;
    }
}