using System.Linq.Expressions;
using System.Reflection;

namespace EfCoreRepository
{
    internal class PropertyInfoByLinqExpressionVisitor : ExpressionVisitor
    {
        private PropertyInfo PropertyInfo { get; set; }

        public static readonly PropertyInfoByLinqExpressionVisitor Instance = new PropertyInfoByLinqExpressionVisitor();

        public PropertyInfo GetPropertyInfo(Expression expression)
        {
            Visit(expression);

            return PropertyInfo;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            PropertyInfo = (PropertyInfo)node.Member;
            
            return base.VisitMember(node);
        }
    }
}