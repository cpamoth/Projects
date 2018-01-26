﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BuildWellWCF
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class BT_SemiAutoEntities : DbContext
    {
        public BT_SemiAutoEntities()
            : base("name=BT_SemiAutoEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Build> Builds { get; set; }
        public virtual DbSet<BuildSource> BuildSources { get; set; }
        public virtual DbSet<Deployment> Deployments { get; set; }
    
        public virtual int AppendToBuildLog(Nullable<int> id, string msg)
        {
            var idParameter = id.HasValue ?
                new ObjectParameter("Id", id) :
                new ObjectParameter("Id", typeof(int));
    
            var msgParameter = msg != null ?
                new ObjectParameter("Msg", msg) :
                new ObjectParameter("Msg", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("AppendToBuildLog", idParameter, msgParameter);
        }
    }
}
