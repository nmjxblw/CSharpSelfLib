using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface NestedInteger
{

	// @return true if this NestedInteger holds a single integer, rather than a nested list.
	bool IsInteger();

	// @return the single integer that this NestedInteger holds, if it holds a single integer
	// Return null if this NestedInteger holds a nested list
	int GetInteger();

	// @return the nested list that this NestedInteger holds, if it holds a nested list
	// Return null if this NestedInteger holds a single integer
	System.Collections.Generic.IList<NestedInteger> GetList();
}