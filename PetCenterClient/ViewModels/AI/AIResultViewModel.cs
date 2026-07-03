namespace PetCenterClient.ViewModels.AI
{
    public class AIResultViewModel
    {
        public string DiseaseName { get; set; }

        public float Confidence { get; set; }

        public string Description { get; set; }

        public string Recommendation { get; set; }

        public string ImageUrl { get; set; }
    }
}
