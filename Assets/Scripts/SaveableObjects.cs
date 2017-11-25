using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class SaveableObjects {

    [System.Serializable]
    class SaveableTrack
    {
        public string name;
        public byte[] texture;
        public SerializableVector3[] trackPoints;
        public int texWidth;
        public int texHeight;

        public SaveableTrack(string name, Texture2D texture, List<TrackPoint> trackPoints)
        {
            this.name = name;
            this.texture = texture.EncodeToPNG();
            this.trackPoints = new SerializableVector3[trackPoints.Count];
            texWidth = texture.width;
            texHeight = texture.height;

            for (int i = 0; i < trackPoints.Count; i++)
            {
                this.trackPoints[i] = new SerializableVector3(trackPoints[i].position);
            }
        }
    }


    public static bool SaveTrack(Track track)
    {
        try
        {
            SaveableTrack saveableTrack = new SaveableTrack(track.trackName, track.texture, track.trackPoints);
            BinaryFormatter bf = new BinaryFormatter();
            Directory.CreateDirectory(Application.persistentDataPath + "/Tracks/");
            FileStream file = File.Create(Application.persistentDataPath + "/Tracks/" + saveableTrack.name + ".trk");
            bf.Serialize(file, saveableTrack);
            file.Close();
            return true;
        }
        catch (System.Exception e)
        {
            Debug.Log(e); 
            return false;
        }

    }

    public static Track LoadTrack(string name)
    {
        try
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/Tracks/" + name + ".trk", FileMode.Open);
            SaveableTrack saveableTrack = (SaveableTrack)bf.Deserialize(file);
            file.Close();
            Texture2D tex = new Texture2D(saveableTrack.texWidth, saveableTrack.texHeight, TextureFormat.ARGB32, false);
            tex.LoadImage(saveableTrack.texture);
            tex.Apply();
            List<Vector3> trackpoints = new List<Vector3>();
            for (int i = 0; i < saveableTrack.trackPoints.Length; i++)
            {
                trackpoints.Add(saveableTrack.trackPoints[i].GetVector3());
            }

            Track track = new Track(saveableTrack.name, tex, trackpoints);
            return track;

        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            return null;
        }
    }

    [System.Serializable]
    public class SaveableNeuralNetwork
    {
        public string name;
        SaveableGenes genes;
        int inputs;
        int outputs;
        Genome.MutParameters mutPar; 

        public SaveableNeuralNetwork(string name, Genome genome)
        {
            this.name = name;
            genes = new SaveableGenes(genome);
            inputs = genome.NumInputs();
            outputs = genome.NumOutPuts();
            mutPar = genome.mutPar;
        }

        public Genome GetGenome()
        {
            List<ConnectionGene> connections = new List<ConnectionGene>();
            List<NodeGene> nodes = new List<NodeGene>();

            for(int i = 0; i < genes.ID.Length; i++)
            {
                nodes.Add(new NodeGene(genes.type[i], genes.ID[i], new Vector2(genes.splitX[i], genes.splitY[i]), genes.recurrent[i], genes.actResponse[i]));
            }

            for(int i = 0; i < genes.from.Length; i++)
            {
                connections.Add(new ConnectionGene(genes.from[i], genes.to[i], genes.enabled[i], genes.innovNum[i], genes.weight[i], genes.ConnecRecurrent[i]));
            }

            Genome genome = new Genome(0, nodes, connections, inputs, outputs, mutPar);

            return genome;
        }


        public bool Save()
        {
            if (genes == null || name == "")
                return false;

            BinaryFormatter bf = new BinaryFormatter();
            Directory.CreateDirectory(Application.persistentDataPath + "/Artificial Neural Networks/");
            FileStream file = File.Create(Application.persistentDataPath + "/Artificial Neural Networks/" + name + ".ann");
            bf.Serialize(file, this);
            file.Close();

            return true;
        }


        public static Genome LoadNetwork(string name)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/Artificial Neural Networks/" + name + ".ann", FileMode.Open);
            SaveableNeuralNetwork SaveableNetwork = (SaveableNeuralNetwork)bf.Deserialize(file);
            file.Close();

            return SaveableNetwork.GetGenome();
        }
    }

    [System.Serializable]
    public class SaveableGenes
    {
        // NodeGenes
        public int[] ID;
        public NodeType[] type;
        public float[] actResponse;
        public bool[] recurrent;
        public float[] splitX;
        public float[] splitY;

        // ConnectionGenes
        public int[] from;
        public int[] to;
        public float[] weight;
        public bool[] enabled;
        public bool[] ConnecRecurrent;
        public int[] innovNum;

        public SaveableGenes(Genome genome)
        {
            ID = new int[genome.GetPerceptronGenes().Count];
            type = new NodeType[genome.GetPerceptronGenes().Count];
            actResponse = new float[genome.GetPerceptronGenes().Count];
            recurrent = new bool[genome.GetPerceptronGenes().Count];
            splitX = new float[genome.GetPerceptronGenes().Count];
            splitY = new float[genome.GetPerceptronGenes().Count];

            from = new int[genome.GetConnectionGenes().Count];
            to = new int[genome.GetConnectionGenes().Count];
            weight = new float[genome.GetConnectionGenes().Count];
            enabled = new bool[genome.GetConnectionGenes().Count];
            ConnecRecurrent = new bool[genome.GetConnectionGenes().Count];
            innovNum = new int[genome.GetConnectionGenes().Count];

            for(int i = 0; i < genome.GetPerceptronGenes().Count; i++)
            {
                ID[i] = genome.GetPerceptronGenes()[i].ID;
                type[i] = genome.GetPerceptronGenes()[i].type;
                actResponse[i] = genome.GetPerceptronGenes()[i].actResponse;
                recurrent[i] = genome.GetPerceptronGenes()[i].recurrent;
                splitX[i] = genome.GetPerceptronGenes()[i].splitValues.x;
                splitY[i] = genome.GetPerceptronGenes()[i].splitValues.y;
            }

            for (int i = 0; i < genome.GetConnectionGenes().Count; i++)
            {
                from[i] = genome.GetConnectionGenes()[i].from;
                to[i] = genome.GetConnectionGenes()[i].to;
                weight[i] = genome.GetConnectionGenes()[i].weight;
                enabled[i] = genome.GetConnectionGenes()[i].enabled;
                ConnecRecurrent[i] = genome.GetConnectionGenes()[i].recurrent;
                innovNum[i] = genome.GetConnectionGenes()[i].innovNum;
            }

        }
    }

    [System.Serializable]
    public class SerializableVector3
    {
        float x;
        float y;
        float z;

        public SerializableVector3(Vector3 vector3)
        {
            this.x = vector3.x;
            this.y = vector3.y;
            this.z = vector3.z;
        }

        public Vector3 GetVector3()
        {
            return new Vector3(x, y, z);
        }
    }
}
