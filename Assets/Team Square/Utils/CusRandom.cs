using Unity.VisualScripting;
using UnityEngine;

namespace Utils
{
    public static class CusRandom
    {
        
        public static int RangeI(Vector2Int _minMax)
        {
            return Random.Range(_minMax.x, _minMax.y);
        }

        public static int RangeI(int _minMax)
        {
            return Random.Range(-_minMax, _minMax);
        }

        public static float RangeF(Vector2 _minMax)
        {
            return Random.Range(_minMax.x, _minMax.y);
        }

        public static float RangeF(float _minMax)
        {
            return Random.Range(-_minMax, _minMax);
        }

        public static float RangeF(float _min, float _max)
        {
            return Random.Range(_min, _max);
        }

        public static Vector3 RangeV3(Vector3 _min, Vector3 _max)
        {
            return new Vector3(
                Random.Range(_min.x, _max.x),
                Random.Range(_min.y, _max.y),
                Random.Range(_min.z, _max.z)
            );
        }

        public static Vector3 Square(float _minMax)
        {
            return new Vector3(RangeF(_minMax/2), 0, RangeF(_minMax/2));
        }
        
        public static Vector3 Rectangle(Vector3 _minMax)
        {
            return new Vector3(RangeF(-_minMax.x, _minMax.x), 0, RangeF(_minMax.z/2));
        }

        /// <summary>
        /// Returns a random XZ position inside <paramref name="outer"/> but outside <paramref name="min"/>,
        /// both expressed as half-extents (x = half-width, y = half-depth).
        /// Sampling is area-weighted across the four border strips so the distribution is uniform.
        /// </summary>
        public static Vector3 RectangleMin(Vector2 outer, Vector2 min)
        {
            float topBottomArea = 2f * outer.x * (outer.y - min.y);
            float leftRightArea = (outer.x - min.x) * 2f * min.y;
            float totalArea     = 2f * topBottomArea + 2f * leftRightArea;

            float r = Random.Range(0f, totalArea);

            if (r < topBottomArea)
                return new Vector3(RangeF(-outer.x, outer.x), 0f, RangeF(min.y, outer.y));
            r -= topBottomArea;

            if (r < topBottomArea)
                return new Vector3(RangeF(-outer.x, outer.x), 0f, RangeF(-outer.y, -min.y));
            r -= topBottomArea;

            if (r < leftRightArea)
                return new Vector3(RangeF(min.x, outer.x), 0f, RangeF(-min.y, min.y));

            return new Vector3(RangeF(-outer.x, -min.x), 0f, RangeF(-min.y, min.y));
        }
        
        public static Vector3 Squares(Vector3[] _squares)
        {
            Vector3 _xSiseZ = _squares[RangeI(new Vector2Int(0, _squares.Length))];
            float halfSize = _xSiseZ.y / 2f;
            return new Vector3(
                RangeF(_xSiseZ.x - halfSize, _xSiseZ.x + halfSize),
                0,
                RangeF(_xSiseZ.z - halfSize, _xSiseZ.z + halfSize)
            );
        }
        
        public static Vector3 Cube(Vector3 _minMax)
        {
            return new Vector3(RangeF(_minMax.x / 2), RangeF(_minMax.y / 2), RangeF(_minMax.z / 2));
        }
        
        public static Vector3 RandomPlan(Vector2 _weightHeight)
        {
            return new Vector3(RangeF(_weightHeight.x), 0, RangeF(_weightHeight.y));
        }


        public static Vector3 Disk(float _radius)
        {
            float _rgnRadius = Random.Range(0f, _radius);

            return RandomAnglePosition(_rgnRadius);
        }
        

        public static Vector3 Disk(float _radius, bool _isCircle)
        {
            if (!_isCircle)
                return Disk(_radius);

            return RandomAnglePosition(_radius);
        }

        public static Vector3 Disk(Vector2 _radiusMinMax)
        {
            float _rgnRadius = Random.Range(_radiusMinMax.x, _radiusMinMax.y);

            return RandomAnglePosition(_rgnRadius);
        }

        private static Vector3 RandomAnglePosition(float _radius)
        {
            float _angle = Random.Range(0f, Mathf.PI * 2);

            float x = _radius * Mathf.Cos(_angle);
            float y = _radius * Mathf.Sin(_angle);

            return new Vector3(x, 0, y);
        }

        public static int RandomWeighted(float[] weights)
        {
            float totalWeight = 0f;
            for (int i = 0; i < weights.Length; i++)
                totalWeight += Mathf.Max(weights[i], 0f);

            if (totalWeight <= 0f)
            {
                // fallback: return a random index
                return Random.Range(0, weights.Length);
            }

            float randomValue = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += Mathf.Max(weights[i], 0f);
                if (randomValue <= cumulative)
                    return i;
            }

            // fallback: should not reach here
            return weights.Length - 1;
        }
    }
}
