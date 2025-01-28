namespace Chat.Entities
{
    public class UserRelationShip : BaseEntity
    {
        public int UserId { get; set; }
        public int RelatedUserId { get; set; }
    }
}
