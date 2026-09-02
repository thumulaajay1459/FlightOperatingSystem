namespace AIService.RAG
{
    public class VectorDocument
    {
        public string Id { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public Dictionary<string, double> Vector { get; set; } = [];
    }

    public class InMemoryVectorStore
    {
        private readonly List<VectorDocument> _documents = [];

        public void AddDocument(string id, string content, string category)
        {
            var doc = new VectorDocument
            {
                Id       = id,
                Content  = content,
                Category = category,
                Vector   = ComputeTfIdf(Tokenize(content))
            };
            _documents.Add(doc);
        }

        public List<VectorDocument> Search(string query, int topK = 3, string? category = null)
        {
            var queryVector = ComputeTfIdf(Tokenize(query));

            var scored = _documents
                .Where(d => category == null || d.Category == category)
                .Select(d => new { Doc = d, Score = CosineSimilarity(queryVector, d.Vector) })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Take(topK)
                .Select(x => x.Doc)
                .ToList();

            return scored;
        }

        private static List<string> Tokenize(string text) =>
            text.ToLower()
                .Split([' ', ',', '.', '?', '!', '\n', '\r', '-', '(', ')'], StringSplitOptions.RemoveEmptyEntries)
                .Where(t => t.Length > 2)
                .ToList();

        private static Dictionary<string, double> ComputeTfIdf(List<string> tokens)
        {
            var tf = new Dictionary<string, double>();
            foreach (var token in tokens)
                tf[token] = tf.GetValueOrDefault(token) + 1;

            foreach (var key in tf.Keys.ToList())
                tf[key] /= tokens.Count;

            return tf;
        }

        private static double CosineSimilarity(Dictionary<string, double> a, Dictionary<string, double> b)
        {
            var dot     = a.Keys.Where(b.ContainsKey).Sum(k => a[k] * b[k]);
            var magA    = Math.Sqrt(a.Values.Sum(v => v * v));
            var magB    = Math.Sqrt(b.Values.Sum(v => v * v));
            return (magA == 0 || magB == 0) ? 0 : dot / (magA * magB);
        }
    }
}
