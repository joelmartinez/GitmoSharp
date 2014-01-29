using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace GitmoSharp {
    /// <summary> Handles creating zip archives of git repositories.</summary>
    internal class Zipper {
        private string outpath;
        private string id;

        /// <summary>Path to the configuration file (json) that stores the date of creation of the archive</summary>
        public string ConfigFilePath { get { return Path.Combine(this.outpath, this.id + "_archive.config"); } }

        /// <summary>Path to the archive itself.</summary>
        public string ArchiveFilePath { get { return Path.Combine(this.outpath, this.id + ".zip"); } }

        /// <summary>Whether the archive file exists.</summary>
        public bool DoesArchiveExist { get { return File.Exists(this.ArchiveFilePath); } }

        public Zipper(string id, string rootPath)
        {
            this.id = id;
            this.outpath = rootPath;


            if (!Directory.Exists(this.outpath)) Directory.CreateDirectory(this.outpath);
        }

        /// <summary>Dictates whether the archive needs to be created.</summary>
        /// <param name="lastUpdated">The last time that the source content was updated.</param>
        public bool DoesArchiveRequireRebuilding(DateTimeOffset lastUpdated)
        {
            if (!this.DoesArchiveExist) return true;

            ArchiveMeta meta;

            if (File.Exists(this.ConfigFilePath)) {
                string contents = File.ReadAllText(this.ConfigFilePath);
                meta = Deserialize(contents);
            }
            else {
                // if the config file doesn't exist, we should rebuild;
                return true;
            }

            return meta.DateCreated < lastUpdated;

        }

        /// <summary>Creates the archive</summary>
        /// <param name="sourcePath">The path to create the archive from.</param>
        public void WriteArchive(string sourcePath)
        {
            if (File.Exists(this.ArchiveFilePath)) File.Delete(this.ArchiveFilePath);

            FastZip fastZip = new FastZip();
            fastZip.CreateZip(this.ArchiveFilePath, sourcePath, true, null);

            ArchiveMeta meta = new ArchiveMeta();
            string contents = Serialize(meta);

            File.WriteAllText(this.ConfigFilePath, contents);

        }

        private ArchiveMeta Deserialize(string json)
        {
            var memoryStream = new MemoryStream(Encoding.Unicode.GetBytes(json));
            var serializer = new DataContractJsonSerializer(typeof(ArchiveMeta));
            var meta = (ArchiveMeta)serializer.ReadObject(memoryStream);
            return meta;
        }

        private string Serialize(ArchiveMeta meta)
        {
            var serializer = new DataContractJsonSerializer(typeof(ArchiveMeta));
            var stream = new MemoryStream();
            serializer.WriteObject(stream, meta);
            stream.Position = 0;
            var sr = new StreamReader(stream);
            return sr.ReadToEnd();
        }

        [DataContract]
        internal class ArchiveMeta {
            [DataMember]
            public DateTimeOffset DateCreated = DateTimeOffset.Now;
        }
    }
}
