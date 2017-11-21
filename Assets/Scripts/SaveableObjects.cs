using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class SaveableObjects : MonoBehaviour {

    [System.Serializable]
    public class SavedTrack
    {
        public string name;
        public float[] textureR;
        public float[] textureG;
        public float[] textureB;
        public float[] textureA;

        public int width;
        public int height;

        public SerializableVector3[] trackPoints;

        public SavedTrack(string name, Texture2D texture, TrackPoints trackPoints)
        {
            this.name = name;

            textureR = new float[texture.width * texture.height];
            textureG = new float[texture.width * texture.height];
            textureB = new float[texture.width * texture.height];
            textureA = new float[texture.width * texture.height];

            for (int i = 0; i < texture.width; i++)
            {
                for (int j = 0; j < texture.height; j++)
                {
                    textureR[i * texture.height + j] = texture.GetPixel(i, j).r;
                    textureG[i * texture.height + j] = texture.GetPixel(i, j).g;
                    textureB[i * texture.height + j] = texture.GetPixel(i, j).b;
                    textureA[i * texture.height + j] = texture.GetPixel(i, j).a;
                }
            }

            width = texture.width;
            height = texture.height;

            this.trackPoints = new SerializableVector3[trackPoints.transform.childCount];

            for (int i = 0; i < trackPoints.transform.childCount; i++)
            {
                this.trackPoints[i] = new SerializableVector3(trackPoints.transform.GetChild(i).transform.position);
            }
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
