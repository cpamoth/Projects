using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace BuildWellWCF
{
    public class BuildWellWCF : IBuildWellWCF
    {
        public int SaveBuild(string binaryRev, string log, string changeControl, string status)
        {
            Build b = new Build();
            b.BinaryRevision = binaryRev;
            b.BuildDate = DateTime.Now;
            b.BuildLog = log;
            b.ChangeControl = changeControl;
            b.Status = status;

            using (BT_SemiAutoEntities de = new BT_SemiAutoEntities())
            {
                de.Builds.Add(b);
                de.SaveChanges();
            }

            return b.Id;
        }

        public int UpdateBuildBinaryRevision(int id, string binaryRev)
        {
            Build build;

            using (BT_SemiAutoEntities de = new BT_SemiAutoEntities())
            {
                build = de.Builds.Single(b => b.Id == id);

                if (build != null)
                {
                    build.BinaryRevision = binaryRev;
                    de.SaveChanges();
                }
            }

            return build.Id;
        }

        public int AppendToBuildLog(int id, string msg)
        {
            Build build;

            using (BT_SemiAutoEntities de = new BT_SemiAutoEntities())
            {
                build = de.Builds.Single(b => b.Id == id);

                if (build != null)
                {
                    build.BuildLog += msg;
                    de.SaveChanges();
                }
            }

            return build.Id;
        }

        public int UpdateBuildDate(int id)
        {
            Build build;

            using (BT_SemiAutoEntities de = new BT_SemiAutoEntities())
            {
                build = de.Builds.Single(b => b.Id == id);

                if (build != null)
                {
                    build.BuildDate = DateTime.Now;
                    de.SaveChanges();
                }
            }

            return build.Id;
        }

        public int UpdateBuildLog(int id, string log)
        {
            Build build;

            using (BT_SemiAutoEntities de = new BT_SemiAutoEntities())
            {
                build = de.Builds.Single(b => b.Id == id);

                if (build != null)
                {
                    build.BuildLog = log;
                    de.SaveChanges();
                }
            }

            return build.Id;
        }

        public int UpdateBuildChangeControl(int id, string cc)
        {
            Build build;

            using (BT_SemiAutoEntities de = new BT_SemiAutoEntities())
            {
                build = de.Builds.Single(b => b.Id == id);

                if (build != null)
                {
                    build.ChangeControl = cc;
                    de.SaveChanges();
                }
            }

            return build.Id;
        }

        public int UpdateBuildStatus(int id, string status)
        {
            Build build;

            using (BT_SemiAutoEntities de = new BT_SemiAutoEntities())
            {
                build = de.Builds.Single(b => b.Id == id);

                if (build != null)
                {
                    build.Status = status;

                    de.SaveChanges();
                }
            }

            return build.Id;
        }

        public void AddBuildReference(int id, string refBy, string repo, string rev, string url)
        {
            BuildSource bs = new BuildSource();
            bs.BuildId = id;
            bs.ReferencedBy = refBy;
            bs.RepositoryType = repo;
            bs.Revision = rev;
            bs.Url = url;

            using (BT_SemiAutoEntities de = new BT_SemiAutoEntities())
            {
                de.BuildSources.Add(bs);
                de.SaveChanges();
            }
        }
    }
}
