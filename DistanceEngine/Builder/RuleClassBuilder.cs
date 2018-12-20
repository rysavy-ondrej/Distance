using System;
using System.Collections.Generic;
using System.Text;

namespace Distance.Engine.Builder
{
    public class RuleClassBuilder
    {
        DiagnosticSpecification.Rule m_rule;
        public RuleClassBuilder(DiagnosticSpecification.Rule rule)
        {
            m_rule = rule;
        }
    }
}
