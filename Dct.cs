class Dct
{
    public static double[,] Dct_Idct(double[,] target, Boolean mode)
    {
        int point = target.GetLength(0);
        //double[,] vector = new double[target.GetLength(0), target.GetLength(1)];
        Array.Copy(target, vector, target.Length);
        Matrix dctMat = MakeMatrix(point);
        Matrix tarMat = new Matrix(vector);
        Matrix result = null;
        //1次元DCT
        if (target.GetLength(1) == 1)
        {
            //DCT
            if (mode)
            {

            }
            //IDCT
            else
            {

            }
        }
        //2次元DCT
        else
        {
            //DCT
            if (mode)
            {

            }
            //IDCT
            else
            {

            }
        }
        return result.m;
    }
    private static Matrix DCTMatrix(int point)
    {
        double[,] result = new double[point, point];
        for (int i = 0; i < point; i++)
        {
            for (int j = 0; j < point; j++)
            {

            }
        }
        return new Matrix(result);
    }
}