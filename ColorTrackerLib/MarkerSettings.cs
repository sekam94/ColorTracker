namespace ColorTrackerLib
{
	public class MarkerSettings
	{
		private float _averageH;
		public float AverageH
		{
			get
			{
				return _averageH;
			}
			set
			{
				_averageH = value;
				if (_averageH < 0)
					_averageH = 0;
				if (_averageH > 360f)
					_averageH = 360f;
			}
		}

		private float _maxDifH;
		public float MaxDifH
		{
			get
			{
				return _maxDifH;
			}
			set
			{
				_maxDifH = value;
				if (_maxDifH < 0)
					_maxDifH = 0;
				if (_maxDifH > 360f)
					_maxDifH = 360f;
			}
		}

		private float _minS;
		public float MinS
		{
			get
			{
				return _minS;
			}
			set
			{
				_minS = value;
				if (_minS < 0)
					_minS = 0;
				if (_minS > 1f)
					_minS = 1f;
			}
		}

		private float _maxS;
		public float MaxS
		{
			get
			{
				return _maxS;
			}
			set
			{
				_maxS = value;
				if (_maxS < 0)
					_maxS = 0;
				if (_maxS > 1f)
					_maxS = 1f;
				
			}
		}
	}
}