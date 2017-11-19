//***************************************************************/
// Possible cases for mesh alighnment in a cell
//**************************************************************/
public class Square
{

    public ControlNode m_TopLeft;
    public ControlNode m_TopRight;
    public ControlNode m_BottomRight;
    public ControlNode m_BottomLeft;

    public Node m_CentreTop;
    public Node m_CentreRight;
    public Node m_CentreBottom; 
    public Node m_CentreLeft;

    public int m_iConfiguration;

    public Square(ControlNode mTopLeft, ControlNode mTopRight, ControlNode mBottomRight, ControlNode mBottomLeft)
    {
        m_TopLeft = mTopLeft;
        m_TopRight = mTopRight;
        m_BottomRight = mBottomRight;
        m_BottomLeft = mBottomLeft;

        m_CentreTop = m_TopLeft.right;
        m_CentreRight = m_BottomRight.above;
        m_CentreBottom = m_BottomLeft.right;
        m_CentreLeft = m_BottomLeft.above;

		// using boolean values

		//8 => 1000
        if (m_TopLeft.active)
            m_iConfiguration += 8;
		//4 = > 0100
        if (m_TopRight.active)
            m_iConfiguration += 4;
		//2 => 0010
        if (m_BottomRight.active)
            m_iConfiguration += 2;
		//1 => 0001
        if (m_BottomLeft.active)
            m_iConfiguration += 1;
    }

}
