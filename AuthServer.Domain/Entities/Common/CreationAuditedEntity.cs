namespace AuthServer.Domain.Entities.Common
{
    public abstract class CreationAuditedEntity<TPrimaryKey>
    {
        public TPrimaryKey Id { get; set; }
        public DateTime? CreatedDate { get; set; } 
        public TPrimaryKey? CreatedBy { get; set; }
    }
}
