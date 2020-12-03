namespace ExtendibleHashing.FileHandlers
{
    abstract class TextFileHandler
    {
        protected const char Separator = ';';

        protected readonly string _path;

        public TextFileHandler(string path)
        {
            _path = path;
        }
    }
}
