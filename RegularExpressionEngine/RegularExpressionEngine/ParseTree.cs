

namespace RegularExpressionEngine
{
  class ParseTree
  {
    public enum NodeType
    {
      Chr,
      Star,
      Question,
      Alter,
      Concat
    }
    
    public NodeType type;
    public char? data;
    public ParseTree left;
    public ParseTree right;

    public ParseTree(NodeType type_, char? data_, ParseTree left_, ParseTree right_)
    {
      type = type_;
      data = data_;
      left = left_;
      right = right_;
    }
  }
}