using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace BuildWellWCF
{
    [ServiceContract]
    public interface IBuildWellWCF
    {
        [OperationContract]
        int SaveBuild(string binaryRev, string log, string changeControl, string status);

        [OperationContract]
        int AppendToBuildLog(int id, string msg);

        [OperationContract]
        int UpdateBuildBinaryRevision(int id, string binaryRev);

        [OperationContract]
        int UpdateBuildDate(int id);

        [OperationContract]
        int UpdateBuildLog(int id, string log);

        [OperationContract]
        int UpdateBuildChangeControl(int id, string cc);

        [OperationContract]
        int UpdateBuildStatus(int id, string status);

        [OperationContract]
        void AddBuildReference(int id, string refBy, string repo, string rev, string url);
    }
}
