using UnityEngine;
using System.Collections;
using System.Linq;
using System;

[AddComponentMenu("Arc Reactor Rays/Ray System")]
public class ArcReactor_Arc : MonoBehaviour {

	public LineRendererInfo[] arcs;
	new public Camera camera;
	public float lifetime;
	public ArcsPlaybackType playbackType = ArcsPlaybackType.once;
	public bool playbackMessages = false;
	public GameObject messageReciever;
	public float elapsedTime = 0;
	public bool playBackward = false;
	public bool freeze = false;
	public float sizeMultiplier = 1;
	public InterpolationType interpolation = InterpolationType.CatmullRom_Splines;
	public EaseInOutOptions easeInOutOptions;
	public Transform[] shapeTransforms;
	public Vector3[] shapePoints;
	public bool[] transformsDestructionFlags;
	public bool closedShape;
	public Vector3 oscillationNormal = Vector3.up;
	public bool localSpaceOcillations = false;
	public float reinitThreshold = 0.5f;
	public int performancePriority = 0;
	public bool customSorting;
	public string sortingLayerName;
	public int sortingOrder;


	const int maxCalcDetalization = 10;

	protected Vector3[] resultingShape;

	protected int oldShapeTransformsSize = 0;
	protected float overlap = 0;
	protected float[] noiseOffsets;
	protected float[] noiseScale;
	protected Vector3[,] arcPoints;
	protected Vector3[,] shiftVectors;
	protected Vector3[,] arcTangents;
	protected Quaternion[,] arcTangentsShift;
	protected Vector3[] shapeTangents;
	protected Vector3[][] vertices;
	protected Vector3[][] oldVertices;
	protected Transform[,] lightsTransforms;
	protected Light[,] lights;
	protected LineRenderer[] lrends;
	protected int[] segmNums;
	protected int[] vertexCount;
	protected int[] oldVertexCount;
	protected int[] lightsCount;
	protected float shapeLength;
	protected float oldShapeLength;
	protected float[] shapeKeyLocations;
	protected float[] shapeKeyNormalizedLocations;
	protected float[] maxStartWidth;
	protected float[] maxEndWidth;
	protected float[] coreCoefs;
	protected Vector3 oscNormal;
	protected LensFlare startFlare;
	protected LensFlare endFlare;
	//protected Mesh[][] emitterMeshes;
	protected ParticleSystem[][] emitterSystems;
	protected ArcReactor_EmitterDestructor[][] emitterDestructors;	

	public float ShapeLength
	{
		get
		{
			return shapeLength;
		}
	}

	public int PerformancePriority
	{
		get
		{
			return performancePriority;
		}
	}

	public enum PropagationType
	{
		instant = 0,
		globalSpaceSpeed = 1,
		localTimeCurve = 2
	}

	public enum ArcsPlaybackType
	{
		once = 0,
		loop = 1,
		pingpong = 2,
		clamp = 3
	}

	public enum InterpolationType
	{
		CatmullRom_Splines = 0,
		Linear = 1
	}

	public enum SpatialNoiseType
	{
		TangentRandomization = 0,
		CubicRandomization = 1,
		BrokenTangentRandomization = 2
	}

	public enum OscillationType
	{
		sine_wave = 0,
		rectangular = 1,
		zigzag = 2
	}

	public enum FadeTypes
	{
		none = 0,
		worldspacePoint = 1,
		relativePoint = 2
	}


	[System.Serializable]
	public class ArcNestingOptions
	{
		public bool Nested = false;
		public int parentArcIndex = 0;
		public bool combinedNesting = false;
		public int secondaryArcIndex = 0;
		public float nestingCoef = 0;
	}


	[System.Serializable]
	public class EaseInOutOptions
	{
		public bool useEaseInOut;
		public AnimationCurve easeInOutCurve;
		public float distance;
	}

	[System.Serializable]
	public class ArcPropagationOptions
	{
		public PropagationType propagationType = PropagationType.instant;
		public float globalSpeed = 1.0f;
		public AnimationCurve timeCurve;
	}

	[System.Serializable]
	public class ArcColorOptions
	{
		public Gradient startColor;
		public bool onlyStartColor = true;
		public Gradient endColor;
		public Gradient coreColor;
		public AnimationCurve coreCurve;
		public float coreJitter;
		public FadeTypes fade;
		public float fadePoint;
	}

	[System.Serializable]
	public class ArcSizeOptions
	{
		public InterpolationType interpolation = InterpolationType.CatmullRom_Splines;
		public AnimationCurve startWidthCurve;
		public bool onlyStartWidth = true;
		public AnimationCurve endWidthCurve;
		public float segmentLength = 10;
		public bool snapSegmentsToShape = false;
		public int numberOfSmoothingSegments = 0;
		public int minNumberOfSegments = 1;
	}

	[System.Serializable]
	public class TextureAnimationOptions
	{
		public Texture shapeTexture;
		public Texture noiseTexture;
		public AnimationCurve noiseCoef;
		public bool animateTexture;
		public float tileSize;
		public float noiseSpeed;
		//public float noisePower;
	}

	[System.Serializable]
	public class ArcSpatialNoiseOptions
	{
		public SpatialNoiseType type = SpatialNoiseType.TangentRandomization;
		public float scale = 0;
		public float scaleMovement = 0;
		public float resetFrequency = 0;
		public int invisiblePriority;
	}

	[System.Serializable]
	public class ArcLightsOptions
	{
		public bool lights = false;
		public float lightsRange = 5;
		public float lightsIntensityMultiplyer = 5;
		public LightRenderMode renderMode = LightRenderMode.Auto;
		public int priority;
	}

	[System.Serializable]
	public class OscillationInfo
	{
		public OscillationType type = OscillationType.sine_wave;
		public bool swirl = false;
		public float planeRotation;
		public float wavelength;
		public bool integerPeriods;
		public WavelengthMetric metric = WavelengthMetric.globalSpace;
		public float amplitude;
		public float phase;
		public float phaseMovementSpeed;
		public int invisiblePriority;
	}
	
	[System.Serializable]
	public class ParticleEmissionOptions
	{
		public bool emit = false;
		public GameObject shurikenPrefab;
		public bool emitAfterRayDeath = false;
		public float particlesPerMeter = 0;
		public AnimationCurve emissionDuringLifetime;
		public AnimationCurve radiusCoefDuringLifetime;
		public AnimationCurve directionDuringLifetime;
		public bool startColorByRay;
		public ParticleRandomizationOptions randomizationOptions;
	}

	[System.Serializable]
	public class ParticleRandomizationOptions
	{
		public float sizeRndCoef = 0;
		public float velocityRndCoef = 0;
		public float angularVelocityRndCoef = 0;
		public float rotationRndCoef = 0;
		public float lifetimeRndCoef = 0;
	}

	public enum WavelengthMetric
	{
		globalSpace = 0,
		localSpace = 1
	}

	[System.Serializable]
	public class ArcFlaresInfo
	{
		public FlareInfo startFlare;
		public FlareInfo endFlare;
		public bool useNoiseMask;
		public AnimationCurve noiseMaskPowerCurve;
	}

	[System.Serializable]
	public class FlareInfo
	{
		public bool enabled = false;
		public Flare flare;
		public float fadeSpeed = 50;
		public float maxBrightness;
		public float maxBrightnessDistance;
		public float minBrightness;
		public float minBrightnessDistance;
		public LayerMask ignoreLayers = (LayerMask)6;
	}

	[System.Serializable]
	public class ShiftCurveInfo
	{
		public AnimationCurve shapeCurve;
		public float curveWidth;
		public float planeRotation;
		public WavelengthMetric metric = WavelengthMetric.globalSpace;
		public float curveLength;
		public bool notAffectedByEaseInOut;
		public int invisiblePriority;
	}

	[System.Serializable]
	public class LineRendererInfo
	{
		public Material material;
		public ArcColorOptions colorOptions;
		public ArcSizeOptions sizeOptions;
		public ArcPropagationOptions propagationOptions;
		public ParticleEmissionOptions[] emissionOptions;
		public ArcSpatialNoiseOptions[] spatialNoise;
		public TextureAnimationOptions textureOptions;
		public ArcLightsOptions lightsOptions;
		public ArcFlaresInfo flaresOptions;
		public ArcNestingOptions nesting;
		public OscillationInfo[] oscillations;
		public ShiftCurveInfo[] shapeCurves;
	}


	public static Vector3 HermiteCurvePoint(float t,Vector3 p0, Vector3 m0, Vector3 p1, Vector3 m1)
	{
		float tsq = t*t;
		float tcub = t*t*t;
		return (2*tcub - 3*tsq + 1) * p0  
			    + (tcub - 2*tsq + t) * m0
				+ (-2*tcub + 3*tsq) * p1
				+ (tcub - tsq) * m1;
	}

	public void FillResultingShape()
	{
		if (resultingShape == null)
			resultingShape = new Vector3[0];
		if (shapePoints != null && shapeTransforms != null)
		{
			if (Mathf.Max(shapeTransforms.Length,shapePoints.Length) != resultingShape.Length)
				Array.Resize(ref resultingShape,Mathf.Max(shapeTransforms.Length,shapePoints.Length));

			for (int i = 0; i < resultingShape.Length; i++)
			{
				if ((shapeTransforms.Length > i) && (shapeTransforms[i] != null))
					resultingShape[i] = shapeTransforms[i].position;
				else
					resultingShape[i] = shapePoints[i];
			}
		}
		else if (shapeTransforms != null)
		{
			if (shapeTransforms.Length != resultingShape.Length)
				Array.Resize(ref resultingShape,shapeTransforms.Length);

			for (int i = 0; i < resultingShape.Length; i++)
				resultingShape[i] = shapeTransforms[i].position;
		}
		else if (shapePoints != null)
		{
			if (shapePoints.Length != resultingShape.Length)
				Array.Resize(ref resultingShape,shapePoints.Length);

			for (int i = 0; i < resultingShape.Length; i++)
				resultingShape[i] = shapePoints[i];
		}
	}


	public static Material GetDefaultMaterial()
	{
		return new Material(Shader.Find("ArcReactor/Additive_core_higlight"));
	}

	public void SetPerformancePriority(int newPriority)
	{
		if (lightsCount != null && performancePriority != newPriority)
		{
			performancePriority = newPriority;
			for (int n = 0; n < arcs.Length; n++)
			{
				if (arcs[n].lightsOptions.lights && (lightsCount[n] > 0))
				{
					for (int i = 0; i < lightsCount[n]; i++)
						lights[n,i].enabled = arcs[n].lightsOptions.priority <= performancePriority;
				}
			}
		}
	}

	protected Vector3 CalculateCurveShift(Vector3 direction, float position, int arcInd)
	{
		Vector3 sumShift = Vector3.zero;
		foreach (ShiftCurveInfo curv in arcs[arcInd].shapeCurves)
		{
			if (lrends[arcInd].isVisible || curv.invisiblePriority <= performancePriority)
			{
				float shift;
				if (curv.metric == WavelengthMetric.localSpace)
					shift = curv.shapeCurve.Evaluate(position/shapeLength) * curv.curveWidth;
				else
					shift = curv.shapeCurve.Evaluate(position/curv.curveLength) * curv.curveWidth;

				Quaternion rot;
				rot = Quaternion.AngleAxis(curv.planeRotation,direction);
				Vector3 normal = Vector3.Cross(direction,oscNormal);
				if (curv.notAffectedByEaseInOut)
					sumShift += rot * normal.normalized * shift;
				else
					sumShift += rot * normal.normalized * shift * GetShiftCoef(position/shapeLength);
			}
		}
		return sumShift * sizeMultiplier;
	}




	protected Vector3 CalculateOscillationShift(Vector3 direction, float position, int arcInd)
	{
		Vector3 sumShift = Vector3.zero;
		foreach (OscillationInfo osc in arcs[arcInd].oscillations)
		{
			if (lrends[arcInd].isVisible || osc.invisiblePriority <= performancePriority)
			{
				float wavelength = osc.wavelength * sizeMultiplier;
				float effectiveWavelength = wavelength;
				if (osc.integerPeriods && osc.metric == WavelengthMetric.globalSpace)
					effectiveWavelength = shapeLength/Mathf.Ceil(shapeLength/wavelength);
				if (osc.integerPeriods && osc.metric == WavelengthMetric.localSpace)
					effectiveWavelength = 1/Mathf.Ceil(1/wavelength);
				float angle;
				if (osc.metric == WavelengthMetric.globalSpace)
					angle = osc.phase*Mathf.Deg2Rad + (position - effectiveWavelength*((int)(position/effectiveWavelength)))/effectiveWavelength * Mathf.PI * 2;
				else
					angle = osc.phase*Mathf.Deg2Rad + (position/shapeLength - effectiveWavelength*((int)(position/shapeLength/effectiveWavelength)))/effectiveWavelength * Mathf.PI * 2;

				float shift;
				switch (osc.type)
				{
				case OscillationType.sine_wave:
					shift = osc.amplitude * Mathf.Sin (angle);
					break;
				case OscillationType.rectangular:
					if ((angle*Mathf.Rad2Deg)%360 > 180)
						shift = -osc.amplitude;
					else
						shift = osc.amplitude;
					break;
				case OscillationType.zigzag:
					shift = osc.amplitude * (Mathf.Abs(((angle*Mathf.Rad2Deg)%180)/45-2)-1);
					break;
				default:
					shift = 0;
					break;
				}
				Quaternion rot;
				rot = Quaternion.AngleAxis(osc.planeRotation,direction);
				Vector3 normal = Vector3.Cross(direction,oscNormal);
				sumShift += rot * normal.normalized * shift;
				if (osc.swirl)
				{
					if (osc.metric == WavelengthMetric.globalSpace)
						angle = (osc.phase+90)*Mathf.Deg2Rad + (position - effectiveWavelength*((int)(position/effectiveWavelength)))/effectiveWavelength * Mathf.PI * 2;
					else
						angle = (osc.phase+90)*Mathf.Deg2Rad + (position/shapeLength - effectiveWavelength*((int)(position/shapeLength/effectiveWavelength)))/effectiveWavelength * Mathf.PI * 2;
					switch (osc.type)
					{
					case OscillationType.sine_wave:
						shift = osc.amplitude * Mathf.Sin (angle);
						break;
					case OscillationType.rectangular:
						if ((angle*Mathf.Rad2Deg)%360 > 180)
							shift = -osc.amplitude;
						else
							shift = osc.amplitude;
						break;
					case OscillationType.zigzag:
						shift = osc.amplitude * (Mathf.Abs(((angle*Mathf.Rad2Deg)%180)/45-2)-1);
						break;
					default:
						shift = 0;
						break;
					}
					rot = Quaternion.AngleAxis(osc.planeRotation+90,direction);
					sumShift += rot * normal.normalized * shift;
				}
			}
		}
		return sumShift * sizeMultiplier;
	}


	protected void CalculateShape()
	{
		FillResultingShape();
		if (oldShapeTransformsSize != resultingShape.Length)
		{
			SetShapeArrays();
		}

		if (closedShape)
		{
			shapeLength = 0;

			for (int i = 0; i < resultingShape.Length-1; i++)
			{
				shapeKeyLocations[i] = shapeLength;
				shapeLength += (resultingShape[i] - resultingShape[i+1]).magnitude;
			}
			shapeKeyLocations[resultingShape.Length-1] = shapeLength;

			float closeLoopLength = (resultingShape[0] - resultingShape[resultingShape.Length - 1]).magnitude;
			shapeLength += closeLoopLength;
			shapeKeyLocations[resultingShape.Length] = shapeLength;

			shapeLength += overlap;
		}
		else
		{
			shapeLength = 0;
			
			for (int i = 0; i < resultingShape.Length-1; i++)
			{
				shapeKeyLocations[i] = shapeLength;
				shapeLength += (resultingShape[i] - resultingShape[i+1]).magnitude;
			}
			shapeKeyLocations[resultingShape.Length-1] = shapeLength;
		}

		for (int i = 0; i < shapeKeyLocations.Length; i++)		
			shapeKeyNormalizedLocations[i] = shapeKeyLocations[i]/shapeLength;


		switch (interpolation)
		{
		case InterpolationType.CatmullRom_Splines:
			if (closedShape)
			{
				for (int i = 0; i < resultingShape.Length; i++)
				{
					shapeTangents[i] = (resultingShape[AddCyclicShift(i,1,resultingShape.Length-1)] - resultingShape[AddCyclicShift(i,-1,resultingShape.Length-1)])/2;
				}
			}
			else
			{
				shapeTangents[0] = resultingShape[1] - resultingShape[0];
				shapeTangents[resultingShape.Length-1] = resultingShape[resultingShape.Length-1] - resultingShape[resultingShape.Length-2];
				for (int i = 1; i < resultingShape.Length-1; i++)
				{
					shapeTangents[i] = (resultingShape[i+1] - resultingShape[i-1])/2;
				}
			}
			break;
		}
		if (oldShapeLength == 0 || Mathf.Abs((oldShapeLength-shapeLength)/shapeLength) > reinitThreshold)
		{
			Initialize();
		}
	}

	protected int AddCyclicShift(int a,int b,int size)
	{
		int s = a+b;
		if (s < 0)
			return s + size + 1;
		if (s > size)
			return s - size - 1;
		return s;
	}

	protected float AddCyclicShift(float a,float b,float size)
	{
		float s = a+b;
		if (s < 0)
			return s + size;
		if (s > size)
			return s - size;
		return s;
	}

	protected Quaternion RandomXYQuaternion(float angle)
	{
		if (angle > 0)
			return Quaternion.Euler(new Vector3(UnityEngine.Random.Range(-angle,angle),
		                                    	UnityEngine.Random.Range(-angle,angle),
		                                    	0));
		else
			return Quaternion.identity;
	}

	protected void SetArcShape(int n)
	{
		float overlapCeof = 1 + overlap/shapeLength;
		int closeShapeShift = 1;
		for (int nI = 0; nI < arcs[n].spatialNoise.Length; nI++)
		{
			switch (arcs[n].spatialNoise[nI].type)
			{
			case SpatialNoiseType.CubicRandomization:
				if (UnityEngine.Random.value > arcs[n].spatialNoise[nI].resetFrequency * Time.deltaTime)
				{
					closeShapeShift = 1;
					if (closedShape)
						closeShapeShift = 0;
					for (int i = 0; i < segmNums[n] + closeShapeShift; i++)
					{
						shiftVectors[n,i] += RandomVector3(arcs[n].spatialNoise[nI].scaleMovement*Time.deltaTime*60) * GetShiftCoef((float)i/segmNums[n]);
					}
				}
				else
				{
					ResetArcNoise(n,nI);
				}
				break;
			case SpatialNoiseType.TangentRandomization:
				if (UnityEngine.Random.value > arcs[n].spatialNoise[nI].resetFrequency * Time.deltaTime)
				{
					closeShapeShift = 1;
					if (closedShape)
						closeShapeShift = 0;
					for (int i = 0; i < segmNums[n] + closeShapeShift; i++)
					{
						arcTangentsShift[n,i*2] = arcTangentsShift[n,i*2] * RandomXYQuaternion(arcs[n].spatialNoise[nI].scaleMovement * GetShiftCoef((float)i/segmNums[n]));
						arcTangentsShift[n,i*2+1] = arcTangentsShift[n,i*2];
					}
				}
				else
				{
					ResetArcNoise(n,nI);
				}
				break;
			case SpatialNoiseType.BrokenTangentRandomization:
				if (UnityEngine.Random.value > arcs[n].spatialNoise[nI].resetFrequency * Time.deltaTime)
				{
					closeShapeShift = 1;
					if (closedShape)
						closeShapeShift = 0;
					for (int i = 0; i < segmNums[n] + closeShapeShift; i++)
					{
						arcTangentsShift[n,i*2] = arcTangentsShift[n,i*2] * RandomXYQuaternion(arcs[n].spatialNoise[nI].scaleMovement * GetShiftCoef((float)i/segmNums[n]));
						arcTangentsShift[n,i*2+1] = arcTangentsShift[n,i*2+1] * RandomXYQuaternion(arcs[n].spatialNoise[nI].scaleMovement * GetShiftCoef((float)i/segmNums[n]));
					}
				}
				else
				{
					ResetArcNoise(n,nI);
				}
				break;
			}
		}

		closeShapeShift = 1;
		if (closedShape)
			closeShapeShift = 0;


		if (arcs[n].nesting.Nested && !arcs[n].nesting.combinedNesting)
			for (int i = 0; i < segmNums[n] + closeShapeShift; i++)
				arcPoints[n,i] = GetArcPoint((float)i/segmNums[n]*overlapCeof,arcs[n].nesting.parentArcIndex) + shiftVectors[n,i] * sizeMultiplier;
		else if (arcs[n].nesting.Nested && arcs[n].nesting.combinedNesting)
			for (int i = 0; i < segmNums[n] + closeShapeShift; i++)
				arcPoints[n,i] = Vector3.Lerp(GetArcPoint((float)i/segmNums[n]*overlapCeof,arcs[n].nesting.parentArcIndex),
				                              GetArcPoint(Mathf.Clamp01((float)i/segmNums[n]*overlapCeof-0.001f),arcs[n].nesting.secondaryArcIndex),
				                              arcs[n].nesting.nestingCoef) + shiftVectors[n,i] * sizeMultiplier;
		else
			for (int i = 0; i < segmNums[n] + closeShapeShift; i++)
				arcPoints[n,i] = CalcShapePoint((float)i/segmNums[n]*overlapCeof) + shiftVectors[n,i] * sizeMultiplier;

		switch (arcs[n].sizeOptions.interpolation)
		{
		case InterpolationType.CatmullRom_Splines:
			if (closedShape)
			{			    
				for (int i = 0; i < segmNums[n]; i++)
				{
					arcTangents[n,i] = (arcPoints[n,AddCyclicShift(i,1,segmNums[n]-1)] - arcPoints[n,AddCyclicShift(i,-1,segmNums[n]-1)])/2;
				}
			}
			else
			{
				arcTangents[n,0] = arcPoints[n,1] - arcPoints[n,0];
				arcTangents[n,segmNums[n]] = arcPoints[n,segmNums[n]] - arcPoints[n,segmNums[n]-1];
				for (int i = 1; i < segmNums[n]; i++)
				{
					arcTangents[n,i] = (arcPoints[n,i+1] - arcPoints[n,i-1])/2;
				}
			}
			break;
		}
	}



	protected Vector3 CalcArcPoint(float point,int n)
	{
		int st = 0;
		int end = 1;
		if (closedShape)
		{
			st = Mathf.FloorToInt(point*segmNums[n]);
			if (point == 1)
				st -= 1;
			if (st == segmNums[n]-1)
				end = 0;
			else
				end = st + 1;			
		}
		else
		{
			st = Mathf.FloorToInt(point*segmNums[n]);
			if (point != 1)
				end = st + 1;
			else
			{
				end = st;
				st -= 1;
			}
		}


		switch (arcs[n].sizeOptions.interpolation)
		{
		case InterpolationType.CatmullRom_Splines:
			return HermiteCurvePoint(point*segmNums[n] - st,arcPoints[n,st],arcTangentsShift[n,st*2]*arcTangents[n,st],arcPoints[n,end],arcTangentsShift[n,end*2+1]*arcTangents[n,end]);
			//break;
		case InterpolationType.Linear:
			return arcPoints[n,st] + (arcPoints[n,end] - arcPoints[n,st])*(point*segmNums[n] - st);
			//break;
		default:
			return arcPoints[n,st] + (arcPoints[n,end] - arcPoints[n,st])*(point*segmNums[n] - st);
			//break;
		}
	}
	

	public Vector3 CalcShapePoint(float point)
	{
		//point = PointShift (point);
		float pos = point * shapeLength;
		int stTr = 0;
		int endTr = 1;
		float localPos = 0;
		for (int i = 0; i < shapeKeyLocations.Length-1; i++)
		{
			if (pos > shapeKeyLocations[i] && pos <= shapeKeyLocations[i+1])
			{
				stTr = i;
				endTr = i+1;
				localPos = 1 - (shapeKeyLocations[i+1]-pos)/(shapeKeyLocations[i+1]-shapeKeyLocations[i]);
				break;
			}
		}

		if (closedShape && endTr == shapeKeyLocations.Length-1)
		{
			stTr = resultingShape.Length-1;
			endTr = 0;
		}

		switch (interpolation)
		{
		case InterpolationType.CatmullRom_Splines:
			return HermiteCurvePoint(localPos,resultingShape[stTr],shapeTangents[stTr],resultingShape[endTr],shapeTangents[endTr]);
		case InterpolationType.Linear:
			return resultingShape[stTr] + (resultingShape[endTr] - resultingShape[stTr])*localPos;
		}
		return Vector3.zero;
	}

	public Vector3 GetArcPoint(float point,int arcIndex)
	{
		float pos = point * (vertexCount[arcIndex]-1);
		int ind1 = Mathf.Clamp(Mathf.FloorToInt(pos),0,vertexCount[arcIndex]-1);
		int ind2 = Mathf.Clamp(Mathf.CeilToInt(pos),0,vertexCount[arcIndex]-1);
		float koef = pos - Mathf.Floor(pos);
		Vector3 vert1;
		Vector3 vert2;
		if (vertices[arcIndex][ind1] == Vector3.zero)
			vert1 = CalcArcPoint(point,arcIndex);
		else
			vert1 = vertices[arcIndex][ind1];
		if (vertices[arcIndex][ind2] == Vector3.zero)
			vert2 = CalcArcPoint(point,arcIndex);
		else
			vert2 = vertices[arcIndex][ind2];
		return vert1*(1-koef) + vert2*koef;
	}

	public Vector3 GetOldArcPoint(float point,int arcIndex)
	{
		float pos = point * (oldVertexCount[arcIndex]-1);
		int ind1 = Mathf.Clamp(Mathf.FloorToInt(pos),0,oldVertexCount[arcIndex]-1);
		int ind2 = Mathf.Clamp(Mathf.CeilToInt(pos),0,oldVertexCount[arcIndex]-1);
		float koef = pos - Mathf.Floor(pos);
		Vector3 oldVert1;
		Vector3 oldVert2;
		if (oldVertices[arcIndex][ind1] == Vector3.zero)
			oldVert1 = CalcArcPoint(point,arcIndex);
		else
			oldVert1 = oldVertices[arcIndex][ind1];
		if (oldVertices[arcIndex][ind2] == Vector3.zero)
			oldVert2 = CalcArcPoint(point,arcIndex);
		else
			oldVert2 = oldVertices[arcIndex][ind2];
		return oldVert1*(1-koef) + oldVert2*koef;
	}


	public float GetShiftCoef(float point)
	{
		if (easeInOutOptions.useEaseInOut)
		{
			float length = point*shapeLength;
			if (length > easeInOutOptions.distance/2 && length < shapeLength - easeInOutOptions.distance/2)
				return easeInOutOptions.easeInOutCurve.Evaluate(0.5f);
			else
			{
				if (length < easeInOutOptions.distance/2)
					return easeInOutOptions.easeInOutCurve.Evaluate(length/easeInOutOptions.distance);
				else
					return easeInOutOptions.easeInOutCurve.Evaluate(1-(shapeLength - length)/easeInOutOptions.distance);
			}
		}
		else
			return 1;
	}


	public void ResetArc(int n)
	{
		float point;
		for (int i = 0; i < arcs[n].spatialNoise.Length; i++)
		{
			ResetArcNoise(n,i);
		}
		/*
		for (int i = 0; i <= segmNums[n]; i++)
		{
			point = (float)i/segmNums[n];
			if (arcs[n].nesting.Nested)
				arcPoints[n,i] = GetArcPoint(point,arcs[n].nesting.parentArcIndex) + shiftVectors[n,i];
			else
				arcPoints[n,i] = CalcShapePoint(point) + shiftVectors[n,i];
		}
		*/
		if (arcs[n].nesting.Nested && !arcs[n].nesting.combinedNesting)
		{
			for (int i = 0; i < segmNums[n]; i++)			
			{
				point = (float)i/segmNums[n];
				arcPoints[n,i] = GetArcPoint(point,arcs[n].nesting.parentArcIndex) + shiftVectors[n,i] * sizeMultiplier;
			}
		}
		else if (arcs[n].nesting.Nested && arcs[n].nesting.combinedNesting)
		{
			for (int i = 0; i < segmNums[n]; i++)
			{
				point = (float)i/segmNums[n];
				arcPoints[n,i] = Vector3.Lerp(GetArcPoint(point,arcs[n].nesting.parentArcIndex),
				                              GetArcPoint(Mathf.Clamp01(point-0.001f),arcs[n].nesting.secondaryArcIndex),
				                              arcs[n].nesting.nestingCoef) + shiftVectors[n,i] * sizeMultiplier;
			}
		}
		else
		{
			for (int i = 0; i < segmNums[n]; i++)
			{
				point = (float)i/segmNums[n];
				arcPoints[n,i] = CalcShapePoint(point) + shiftVectors[n,i] * sizeMultiplier;
			}
		}

	}

	public void ResetArcNoise(int n, int noiseInd)
	{
		switch (arcs[n].spatialNoise[noiseInd].type)
		{
		case SpatialNoiseType.CubicRandomization:
			for (int i = 0; i <= segmNums[n]; i++)			
				shiftVectors[n,i] = RandomVector3(arcs[n].spatialNoise[noiseInd].scale) * GetShiftCoef((float)i/segmNums[n]);
			break;
		case SpatialNoiseType.TangentRandomization:
			for (int i = 0; i <= segmNums[n]; i++)			
			{
				arcTangentsShift[n,i*2] = RandomXYQuaternion(arcs[n].spatialNoise[noiseInd].scale * GetShiftCoef((float)i/segmNums[n]));
				arcTangentsShift[n,i*2+1] = arcTangentsShift[n,i*2];
			}
			break;
		case SpatialNoiseType.BrokenTangentRandomization:
			for (int i = 0; i <= segmNums[n]; i++)			
			{
				arcTangentsShift[n,i*2] = RandomXYQuaternion(arcs[n].spatialNoise[noiseInd].scale * GetShiftCoef((float)i/segmNums[n]));
				arcTangentsShift[n,i*2+1] = RandomXYQuaternion(arcs[n].spatialNoise[noiseInd].scale * GetShiftCoef((float)i/segmNums[n]));
			}
			break;
		}
	}
	

	protected float GetFlareBrightness(Vector3 cameraPosition,Vector3 flarePosition, FlareInfo flInfo, float multiplier = 1)
	{
		float distance = Mathf.Clamp((cameraPosition - flarePosition).magnitude,flInfo.maxBrightnessDistance,flInfo.minBrightnessDistance) - flInfo.maxBrightnessDistance;
		return Mathf.Lerp (flInfo.maxBrightness,flInfo.minBrightness,distance / (flInfo.minBrightnessDistance - flInfo.maxBrightnessDistance)) * multiplier;
	}

	protected void SetFlares(int n)
	{
		float multiplier = 1;
		if (arcs[n].flaresOptions.startFlare.enabled)
		{
			startFlare.transform.position = resultingShape[0];

			if (arcs[n].flaresOptions.useNoiseMask)
				multiplier = arcs[n].flaresOptions.noiseMaskPowerCurve.Evaluate(noiseOffsets[n]);

			startFlare.brightness = GetFlareBrightness(camera.transform.position,resultingShape[0],arcs[n].flaresOptions.startFlare,
			                                           arcs[n].sizeOptions.startWidthCurve.Evaluate(elapsedTime/lifetime)/maxStartWidth[n]) * multiplier;
			startFlare.color = arcs[n].colorOptions.startColor.Evaluate(elapsedTime/lifetime);
		}
		if (arcs[n].flaresOptions.endFlare.enabled)
		{
			endFlare.transform.position = resultingShape[resultingShape.Length-1];

			if (arcs[n].flaresOptions.useNoiseMask)
				multiplier = arcs[n].flaresOptions.noiseMaskPowerCurve.Evaluate(AddCyclicShift(noiseScale[n]-Mathf.Floor(noiseScale[n]),noiseOffsets[n],1));

			if (arcs[n].sizeOptions.onlyStartWidth)
				endFlare.brightness = GetFlareBrightness(camera.transform.position,resultingShape[resultingShape.Length - 1],arcs[n].flaresOptions.endFlare,
				                                         arcs[n].sizeOptions.startWidthCurve.Evaluate(elapsedTime/lifetime)/maxStartWidth[n]) * multiplier;
			else
				endFlare.brightness = GetFlareBrightness(camera.transform.position,resultingShape[resultingShape.Length - 1],arcs[n].flaresOptions.endFlare,
				                                         arcs[n].sizeOptions.endWidthCurve.Evaluate(elapsedTime/lifetime)/maxEndWidth[n]) * multiplier;
			if (arcs[n].colorOptions.onlyStartColor)			
				endFlare.color = arcs[n].colorOptions.startColor.Evaluate(elapsedTime/lifetime);
			else
				endFlare.color = arcs[n].colorOptions.endColor.Evaluate(elapsedTime/lifetime);
		}
	}


	protected void Initialize ()
	{
		oldShapeLength = shapeLength;

		bool anyLights = false;

		for(int n = 0; n < arcs.Length; n++)
		{
			//Particle emitter initialization
			for (int q = 0; q < arcs[n].emissionOptions.Length; q++)
			{
				if (emitterSystems[n][q] == null && arcs[n].emissionOptions[q].shurikenPrefab != null)
				{
					GameObject partGameObject = (GameObject)GameObject.Instantiate(arcs[n].emissionOptions[q].shurikenPrefab);
					partGameObject.name = "EmitterObject "+gameObject.name+" "+n.ToString()+","+q.ToString();
					emitterSystems[n][q] = partGameObject.GetComponent<ParticleSystem>();
					if (emitterSystems[n][q].enableEmission)
						emitterSystems[n][q].enableEmission = false;
					if (!arcs[n].emissionOptions[q].emitAfterRayDeath)
						partGameObject.transform.parent = transform;
					else
					{
						emitterDestructors[n][q] = partGameObject.AddComponent<ArcReactor_EmitterDestructor>();
						emitterDestructors[n][q].partSystem = emitterSystems[n][q];
						emitterDestructors[n][q].enabled = false;
					}
					partGameObject.transform.position = transform.position;
					partGameObject.transform.rotation = transform.rotation;
				}
			}


			//Lights initialization
			if (arcs[n].lightsOptions.lights)
			{
				for (int i = 0; i < lightsCount[n]; i++)
				{
					Destroy(lights[n,i].gameObject);
				}
			}
			anyLights |= arcs[n].lightsOptions.lights;
			lightsCount[n] = Mathf.Max ((int)(shapeLength * 2 / arcs[n].lightsOptions.lightsRange + 1),2);

			//Segment and vertex initialization
			segmNums[n] = Mathf.Max((int)(shapeLength / (arcs[n].sizeOptions.segmentLength * sizeMultiplier))+arcs[n].sizeOptions.minNumberOfSegments,2);			
			vertexCount[n] = segmNums[n]*(arcs[n].sizeOptions.numberOfSmoothingSegments+1)+1;
			oldVertexCount[n] = vertexCount[n];
			oldVertices[n] = new Vector3[vertexCount[n]];
			vertices[n] = new Vector3[vertexCount[n]];
			lrends[n].SetVertexCount(vertexCount[n]);

			//Flares placing
			if (arcs[n].flaresOptions.startFlare.enabled && startFlare == null)
			{
				GameObject obj = new GameObject(gameObject.name + "_Start_flare");
				obj.transform.parent = transform;
				startFlare = obj.gameObject.AddComponent<LensFlare>();
				startFlare.flare = arcs[n].flaresOptions.startFlare.flare;
				startFlare.fadeSpeed = arcs[n].flaresOptions.startFlare.fadeSpeed;
			}
			if (arcs[n].flaresOptions.endFlare.enabled && endFlare == null)
			{
				GameObject obj = new GameObject(gameObject.name + "_End_flare");
				obj.transform.parent = transform;
				endFlare = obj.gameObject.AddComponent<LensFlare>();
				endFlare.flare = arcs[n].flaresOptions.endFlare.flare;
				endFlare.fadeSpeed = arcs[n].flaresOptions.endFlare.fadeSpeed;
			}

		}

		//vertices = new Vector3[arcs.Length,vertexCount.Max()];
		arcPoints = new Vector3[arcs.Length,segmNums.Max()+2];
		shiftVectors = new Vector3[arcs.Length,segmNums.Max()+2];
		arcTangents = new Vector3[arcs.Length,segmNums.Max()+2];
		arcTangentsShift = new Quaternion[arcs.Length,segmNums.Max()*2+2];

		for (int n = 0; n < arcs.Length; n++)
		{
			ResetArc(n);
		}

		if (anyLights)
		{
			GameObject lightObject;
			lights = new Light[arcs.Length,lightsCount.Max()];
			lightsTransforms = new Transform[arcs.Length,lightsCount.Max()+1];
			for(int n = 0; n < arcs.Length; n++)
			{			
				if (arcs[n].lightsOptions.lights)
				{
					for (int i = 0; i < lightsCount[n]; i++)
					{
						lightObject = new GameObject("ArcLight");
						lightObject.transform.parent = transform;
						lightsTransforms[n,i] = lightObject.transform;
						lights[n,i] = lightObject.AddComponent<Light>();
						lights[n,i].type = LightType.Point;
						lights[n,i].renderMode = arcs[n].lightsOptions.renderMode;
						lights[n,i].range = arcs[n].lightsOptions.lightsRange;
					}
				}
			}
		}
	}

	protected void SetShapeArrays()
	{
		int shapeLen = Mathf.Max(shapeTransforms.Length,shapePoints.Length);
		oldShapeTransformsSize = shapeLen;
		if (closedShape)
		{
			shapeKeyLocations = new float[shapeLen+1];
			shapeKeyNormalizedLocations = new float[shapeLen+1];
		}
		else
		{
			shapeKeyLocations = new float[shapeLen];
			shapeKeyNormalizedLocations = new float[shapeLen];
		}
		shapeTangents = new Vector3[shapeLen];
	}

	
	void Start () 
	{
		//Checks for correct parameters
		if (Mathf.Max(shapeTransforms.Length, shapePoints.Length) < 2)
		{
			Debug.LogError(gameObject.name + " : There should be at least 2 shape transforms or points for correct shape calculation. Deactivating component.");
			this.enabled = false;
			return;
		}
		if (arcs.Length == 0)
		{
			Debug.LogError(gameObject.name + " : No arcs set up. Deactivating component.");
			this.enabled = false;
			return;
		}
		if (lifetime == 0)		
			Debug.LogWarning(gameObject.name + " : Lifetime set to zero. That's a waste of a perfectly good component.");
		if (oscillationNormal == Vector3.zero)
			Debug.LogWarning(gameObject.name + " : Oscillation normal set to zero. Oscillation planes will be unpredictable.");
		if (easeInOutOptions.useEaseInOut && easeInOutOptions.distance == 0)
			Debug.LogWarning(gameObject.name + " : EaseInOut enabled but it's distance set to zero. It will have no effect except performance hit.");
		for (int i = 0; i < arcs.Length; i++)
		{
			if ((arcs[i].flaresOptions.startFlare.enabled || arcs[i].flaresOptions.endFlare.enabled) && camera == null)
				camera = Camera.main;

			if (arcs[i].colorOptions.startColor.colorKeys.Length == 2
			    && arcs[i].colorOptions.startColor.colorKeys[0].color == new Color(0,0,0,255)
			    && arcs[i].colorOptions.startColor.colorKeys[0].time == 0
			    && arcs[i].colorOptions.startColor.colorKeys[1].color == new Color(0,0,0,255)
			    && arcs[i].colorOptions.startColor.colorKeys[1].time == 1
			    && arcs[i].colorOptions.startColor.alphaKeys.Length == 2
			    && arcs[i].colorOptions.startColor.alphaKeys[0].alpha == 0
			    && arcs[i].colorOptions.startColor.alphaKeys[0].time == 0
			    && arcs[i].colorOptions.startColor.alphaKeys[1].alpha == 0
			    && arcs[i].colorOptions.startColor.alphaKeys[1].time == 1)
			{
				Debug.LogWarning(gameObject.name + " : Start color gradient has not been assigned to Arc #"+i.ToString()+", arc probably wouldn't be visible. Set color options to see the arc.");
			}

			if (arcs[i].sizeOptions.segmentLength == 0)
			{
				Debug.LogWarning(gameObject.name + " : Segment length of Arc #"+i.ToString()+" is set to zero, arc will always be consisting of only 2 vertexes");
			}

			if (arcs[i].sizeOptions.startWidthCurve.keys.Length == 0 && (arcs[i].sizeOptions.onlyStartWidth || arcs[i].sizeOptions.endWidthCurve.keys.Length == 0))
			{
				Debug.LogWarning(gameObject.name + " : Width curves has not been assigned to Arc #"+i.ToString()+", setting default curves.");
				arcs[i].sizeOptions.startWidthCurve.AddKey(0,0.5f);
				if (!arcs[i].sizeOptions.onlyStartWidth)
					arcs[i].sizeOptions.endWidthCurve.AddKey(0,0.5f);
			}

			if (arcs[i].material == null)
			{
				Debug.LogWarning(gameObject.name + " : Material have not been assigned to Arc #"+i.ToString()+", setting default material.");
				arcs[i].material = GetDefaultMaterial();
			}

			if (arcs[i].nesting.Nested && arcs[i].nesting.parentArcIndex > i)
				Debug.LogWarning(gameObject.name + " : Arc #"+i.ToString()+" is nested to arc with higher index. That's not recommended because of vertex caching.");

			for (int q = 0; q < arcs[i].oscillations.Length; q++)
			{
				if (arcs[i].oscillations[q].amplitude == 0)
				{
					Debug.LogWarning(gameObject.name + " : Amplitude of oscillation #"+q.ToString()+" of Arc #"+i.ToString()+" set to zero. It will have no effect except performance hit");
				}
				if (arcs[i].oscillations[q].wavelength == 0)
				{
					Debug.LogError(gameObject.name + " : Wavelength of oscillation #"+q.ToString()+" of Arc #"+i.ToString()+" set to zero. That makes no mathematical sense. Disabling component");
					this.enabled = false;
					return;
				}
			}
		}



		//Service array initialization, actual data creation happens at Initialize()
		emitterSystems = new ParticleSystem[arcs.Length][];
		emitterDestructors = new ArcReactor_EmitterDestructor[arcs.Length][];
		for (int n = 0; n < arcs.Length; n++)
		{
			emitterSystems[n] = new ParticleSystem[arcs[n].emissionOptions.Length];
			emitterDestructors[n] = new ArcReactor_EmitterDestructor[arcs[n].emissionOptions.Length];
		}
		lrends = new LineRenderer[arcs.Length];
		segmNums = new int[arcs.Length];
		lightsCount = new int[arcs.Length];
		vertexCount = new int[arcs.Length];
		oldVertexCount = new int[arcs.Length];
		noiseOffsets = new float[arcs.Length];
		noiseScale = new float[arcs.Length];
		maxStartWidth = new float[arcs.Length];
		maxEndWidth = new float[arcs.Length];
		coreCoefs = new float[arcs.Length];
		vertices = new Vector3[arcs.Length][];
		oldVertices = new Vector3[arcs.Length][];


		//Init
		SetShapeArrays();

		GameObject rayLineRenderer;
		for(int n = 0; n < arcs.Length; n++)
		{
			rayLineRenderer = new GameObject("ArcLineRenderer");
			rayLineRenderer.transform.parent = transform;
			lrends[n] = rayLineRenderer.AddComponent<LineRenderer>();
			lrends[n].material = arcs[n].material;
			lrends[n].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			lrends[n].receiveShadows = false;
			if (customSorting)
			{
				lrends[n].sortingLayerName = sortingLayerName;
				lrends[n].sortingOrder = sortingOrder;
			}

			//texture setup
			if (arcs[n].textureOptions.shapeTexture != null)
				lrends[n].material.SetTexture("_MainTex",arcs[n].textureOptions.shapeTexture);
			if (arcs[n].textureOptions.noiseTexture != null)
				lrends[n].material.SetTexture("_NoiseMask",arcs[n].textureOptions.noiseTexture);

			//Calculating maximum widths
			float maxWidth = 0;
			if (arcs[n].flaresOptions.startFlare.enabled)			
			{
				for (int i = 0; i <= maxCalcDetalization; i++)
				{
					if (maxWidth < arcs[n].sizeOptions.startWidthCurve.Evaluate((float)i/maxCalcDetalization))
						maxWidth = arcs[n].sizeOptions.startWidthCurve.Evaluate((float)i/maxCalcDetalization);
				}
				maxStartWidth[n] = maxWidth;
			}

			if (arcs[n].flaresOptions.endFlare.enabled)
			{
				if (arcs[n].sizeOptions.onlyStartWidth)
				{
					if (arcs[n].flaresOptions.startFlare.enabled)
						maxEndWidth[n] = maxStartWidth[n];
					else
					{
						for (int i = 0; i <= maxCalcDetalization; i++)
						{
							if (maxWidth < arcs[n].sizeOptions.startWidthCurve.Evaluate((float)i/maxCalcDetalization))
								maxWidth = arcs[n].sizeOptions.startWidthCurve.Evaluate((float)i/maxCalcDetalization);
						}
						maxStartWidth[n] = maxWidth;
						maxEndWidth[n] = maxStartWidth[n];
					}
				}
				else
				{
					maxWidth = 0;
					for (int i = 0; i <= maxCalcDetalization; i++)				
						if (maxWidth < arcs[n].sizeOptions.endWidthCurve.Evaluate((float)i/maxCalcDetalization))
							maxWidth = arcs[n].sizeOptions.endWidthCurve.Evaluate((float)i/maxCalcDetalization);
					maxEndWidth[n] = maxWidth;
				}
			}
		}

		CalculateShape();

		//Adding this system to performance manager if it exists
		if (ArcReactor_Manager.Instance != null)
			ArcReactor_Manager.Instance.AddArcSystem(this);
	}	


	public Vector3 RandomVector3(float range)
	{
		return new Vector3(UnityEngine.Random.Range(-range,range),
		                   UnityEngine.Random.Range(-range,range),
		                   UnityEngine.Random.Range(-range,range));
	}


	void Update()
	{
		//Phase shifting
		for (int n = 0; n < arcs.Length; n++)
		{
			foreach(OscillationInfo osc in arcs[n].oscillations)
			{
				osc.phase += osc.phaseMovementSpeed * Time.deltaTime;
				if (osc.phase > 360)
					osc.phase = osc.phase - 360;
				if (osc.phase < 0)
					osc.phase = osc.phase + 360;
			}
		}

		//Time management
		if (!freeze)
		{
			if (!playBackward)
				elapsedTime += Time.deltaTime;
			else
				elapsedTime -= Time.deltaTime;
		}
		
		if (elapsedTime > lifetime)
		{
			switch (playbackType)
			{
			case ArcsPlaybackType.once:
				for (int i = 0; i < Mathf.Min(shapeTransforms.Length,transformsDestructionFlags.Length); i++)
				{
					if (transformsDestructionFlags[i])
					{
						Destroy(shapeTransforms[i].gameObject);
					}
				}
				for (int n = 0; n < arcs.Length; n++)
				{
					for (int i = 0; i < arcs[n].emissionOptions.Length; i++)
					{
						if (arcs[n].emissionOptions[i].emitAfterRayDeath)
							emitterDestructors[n][i].enabled = true;
					}
				}
				if (playbackMessages)
					messageReciever.SendMessage("ArcReactorPlayback",this);
				Destroy(gameObject);
				break;
			case ArcsPlaybackType.loop:
				elapsedTime -= lifetime;
				if (playbackMessages)
					messageReciever.SendMessage("ArcReactorPlayback",this);
				break;
			case ArcsPlaybackType.pingpong:
				playBackward = true;
				elapsedTime = lifetime;
				if (playbackMessages)
					messageReciever.SendMessage("ArcReactorPlayback",this);
				break;
			case ArcsPlaybackType.clamp:
				elapsedTime = lifetime;
				freeze = true;
				if (playbackMessages)
					messageReciever.SendMessage("ArcReactorPlayback",this);
				break;
			}
						
		}
		if (elapsedTime < 0)
		{
			playBackward = false;
			elapsedTime = 0;
		}
			
}


	public Vector3 GetArcEndPosition(int arcIndex)
	{
		return GetArcPoint(GetArcEndPoint(arcIndex),arcIndex);
	}



	public float GetArcEndPoint(int arcIndex)
	{
		switch (arcs[arcIndex].propagationOptions.propagationType)
		{
		case PropagationType.globalSpaceSpeed:
			return Mathf.Min(vertexCount[arcIndex] * arcs[arcIndex].propagationOptions.globalSpeed * elapsedTime / shapeLength,vertexCount[arcIndex])/vertexCount[arcIndex];
		case PropagationType.localTimeCurve:
			return Mathf.Clamp01(arcs[arcIndex].propagationOptions.timeCurve.Evaluate(elapsedTime/lifetime));
		case PropagationType.instant:
			return 1;
		default:
			return 1;
		}
	}



	
	void LateUpdate () 
	{
		float lifetimePos = elapsedTime/lifetime;
		CalculateShape();
		if (localSpaceOcillations)
			oscNormal = transform.rotation * oscillationNormal;
		else
			oscNormal = oscillationNormal;
		for (int n = 0; n < arcs.Length; n++)
		{
			vertices[n].CopyTo(oldVertices[n],0);

			Color StartColor = arcs[n].colorOptions.startColor.Evaluate(lifetimePos);
			Color EndColor;
			if (arcs[n].colorOptions.onlyStartColor)
				EndColor = StartColor;
			else
				EndColor = arcs[n].colorOptions.endColor.Evaluate(lifetimePos);
			Color coreColor = arcs[n].colorOptions.coreColor.Evaluate(lifetimePos);

			lrends[n].material.SetColor("_StartColor",StartColor);
			lrends[n].material.SetColor("_EndColor",EndColor);

			lrends[n].material.SetColor("_CoreColor",coreColor);

			if (arcs[n].colorOptions.coreJitter > 0)
			{
				coreCoefs[n] = arcs[n].colorOptions.coreCurve.Evaluate(lifetimePos)+UnityEngine.Random.Range(-arcs[n].colorOptions.coreJitter*0.5f,arcs[n].colorOptions.coreJitter*0.5f);
				lrends[n].material.SetFloat("_CoreCoef",coreCoefs[n]);
			}
			else
			{
				coreCoefs[n] = arcs[n].colorOptions.coreCurve.Evaluate(lifetimePos);
				lrends[n].material.SetFloat("_CoreCoef",coreCoefs[n]);
			}

			//Fading
			switch (arcs[n].colorOptions.fade)
			{
			case FadeTypes.none:
				lrends[n].material.SetFloat("_FadeLevel",0.001f);
				break;
			case FadeTypes.relativePoint:
				lrends[n].material.SetFloat("_FadeLevel",Mathf.Max(arcs[n].colorOptions.fadePoint,0.001f));
				break;
			case FadeTypes.worldspacePoint:
				lrends[n].material.SetFloat("_FadeLevel",Mathf.Max(Mathf.Clamp01(arcs[n].colorOptions.fadePoint / shapeLength),0.001f));
				break;
			}

			//Ray size change
			float startWidth = arcs[n].sizeOptions.startWidthCurve.Evaluate(lifetimePos) * sizeMultiplier;
			float endWidth;
			if (arcs[n].sizeOptions.onlyStartWidth)
				endWidth = startWidth;
			else
				endWidth = arcs[n].sizeOptions.endWidthCurve.Evaluate(lifetimePos) * sizeMultiplier;

			lrends[n].SetWidth(startWidth,endWidth);

			float vertexCnt = vertexCount[n];
			switch (arcs[n].propagationOptions.propagationType)
			{
			case PropagationType.globalSpaceSpeed:
				vertexCnt = Mathf.Min(vertexCount[n] * arcs[n].propagationOptions.globalSpeed * elapsedTime / shapeLength,vertexCount[n]);
				lrends[n].SetVertexCount(Mathf.CeilToInt(vertexCnt));
				break;
			case PropagationType.localTimeCurve:
				vertexCnt = Mathf.Min(vertexCount[n] * arcs[n].propagationOptions.timeCurve.Evaluate(lifetimePos),vertexCount[n]);
				lrends[n].SetVertexCount(Mathf.Max(Mathf.CeilToInt(vertexCnt),0));
				break;
			}

			//Texture handling
			if (arcs[n].textureOptions.noiseTexture != null)
			{
				lrends[n].material.SetFloat("_NoiseCoef", arcs[n].textureOptions.noiseCoef.Evaluate(lifetimePos));
				if (arcs[n].textureOptions.animateTexture)
				{
					noiseOffsets[n] += arcs[n].textureOptions.noiseSpeed * Time.deltaTime;
					if (noiseOffsets[n]>1)
						noiseOffsets[n] -= 1;
					if (noiseOffsets[n]<0)
						noiseOffsets[n] += 1;
					noiseScale[n] = vertexCnt/vertexCount[n]*shapeLength / arcs[n].textureOptions.tileSize;
					lrends[n].material.SetTextureScale("_NoiseMask",new Vector2(noiseScale[n],1));
					lrends[n].material.SetTextureOffset("_NoiseMask",new Vector2(noiseOffsets[n],1));
				}
				else
				{
					noiseScale[n] = vertexCnt/vertexCount[n]*shapeLength / arcs[n].textureOptions.tileSize;
					lrends[n].material.SetTextureScale("_NoiseMask",new Vector2(noiseScale[n],1));
				}			
			}

			SetFlares (n);
			SetArcShape(n);
			Vector3 curVertexPos;
			curVertexPos = CalcArcPoint(0,n);
			Vector3 nextVertexPos = Vector3.zero;
			Vector3 direction = Vector3.zero;
			float pos;
			int currentShapeKey = 1;
			for (int curVertex = 0; curVertex < vertexCnt-1; curVertex++)
			{
				pos = (float)curVertex/vertexCount[n];
				
				if (arcs[n].sizeOptions.snapSegmentsToShape && Mathf.Abs(shapeKeyNormalizedLocations[currentShapeKey]-pos)*vertexCount[n] < 0.5)
				{
					pos = shapeKeyNormalizedLocations[currentShapeKey];
					curVertexPos = shapeTransforms[currentShapeKey].position;
					currentShapeKey++;
				}
				nextVertexPos = CalcArcPoint((float)(curVertex+1)/vertexCount[n],n);
				direction = nextVertexPos-curVertexPos;
				vertices[n][curVertex] = curVertexPos
					                    + CalculateOscillationShift(direction,pos*shapeLength,n) * GetShiftCoef(pos)
										+ CalculateCurveShift(direction,pos*ShapeLength,n);
				lrends[n].SetPosition(curVertex,vertices[n][curVertex]);
				curVertexPos = nextVertexPos;				
			}
			if (Mathf.CeilToInt(vertexCnt)>0 && Mathf.CeilToInt(vertexCnt) <= vertexCount[n])
			{
				vertices[n][Mathf.CeilToInt(vertexCnt)-1] = CalculateOscillationShift(direction,shapeLength*(vertexCnt)/vertexCount[n],n) * GetShiftCoef(vertexCnt/vertexCount[n])
				                                           	+ CalcArcPoint(vertexCnt/vertexCount[n],n);
				lrends[n].SetPosition(Mathf.CeilToInt(vertexCnt)-1,vertices[n][Mathf.CeilToInt(vertexCnt)-1]);
			}




			//Particles emissions
			for (int i = 0; i < arcs[n].emissionOptions.Length; i++)
			{
				if (arcs[n].emissionOptions[i].emit)
				{
					ParticleSystem.Particle tmpParticle = new ParticleSystem.Particle();
					int particleCount = (int)(UnityEngine.Random.value + vertexCnt/vertexCount[n] * shapeLength * arcs[n].emissionOptions[i].particlesPerMeter * Time.deltaTime * arcs[n].emissionOptions[i].emissionDuringLifetime.Evaluate(lifetimePos));
					float arcEndPoint = vertexCnt/vertexCount[n];
					float radiusCoef = arcs[n].emissionOptions[i].radiusCoefDuringLifetime.Evaluate(lifetimePos);
					float directionCoef = arcs[n].emissionOptions[i].directionDuringLifetime.Evaluate(lifetimePos);
					float radius;
					float rand = 0;
					Vector3 randomVect = Vector3.one;
					Vector3 spaceShiftVect;
					if (emitterSystems[n][i].simulationSpace == ParticleSystemSimulationSpace.Local)					
						spaceShiftVect = -emitterSystems[n][i].transform.position;
					else
						spaceShiftVect = Vector3.zero;
					Color emitStartColor;
					Color emitEndColor;
					Vector3 emitPos;
					Vector3 emitDir;
					if (arcs[n].emissionOptions[i].startColorByRay)
					{
						emitStartColor = StartColor;
						emitEndColor = EndColor;
					}
					else
					{
						emitStartColor = emitterSystems[n][i].startColor;
						emitEndColor = emitterSystems[n][i].startColor;
					}
					//Debug.Log(emitterSystems[n][i].particleCount);
					for (int q = 1; q <= particleCount; q++)
					{
						rand = 0.001f + UnityEngine.Random.value * (arcEndPoint-0.002f); //get random point without touching exact end of arc
						randomVect = UnityEngine.Random.rotation * Vector3.forward;
						radius = Mathf.Lerp(startWidth,endWidth,rand) * radiusCoef;
						emitPos = GetArcPoint(rand,n);
						emitDir = (GetArcPoint(rand+0.001f,n) - emitPos).normalized;
						tmpParticle.position = Vector3.Lerp(emitPos,GetOldArcPoint(rand,n),UnityEngine.Random.value) + randomVect * radius + spaceShiftVect;
						tmpParticle.startLifetime = emitterSystems[n][i].startLifetime * (1 - arcs[n].emissionOptions[i].randomizationOptions.lifetimeRndCoef + arcs[n].emissionOptions[i].randomizationOptions.lifetimeRndCoef * UnityEngine.Random.value);
						tmpParticle.remainingLifetime = tmpParticle.startLifetime;
						tmpParticle.velocity = (randomVect * (1f - Mathf.Clamp01(Mathf.Abs(directionCoef))) + emitDir * directionCoef) * emitterSystems[n][i].startSpeed * (1 - arcs[n].emissionOptions[i].randomizationOptions.velocityRndCoef + arcs[n].emissionOptions[i].randomizationOptions.velocityRndCoef * UnityEngine.Random.value) * sizeMultiplier;
						tmpParticle.rotation = emitterSystems[n][i].startRotation * (1 - arcs[n].emissionOptions[i].randomizationOptions.rotationRndCoef + arcs[n].emissionOptions[i].randomizationOptions.rotationRndCoef * UnityEngine.Random.value);
						tmpParticle.angularVelocity = (arcs[n].emissionOptions[i].randomizationOptions.rotationRndCoef + arcs[n].emissionOptions[i].randomizationOptions.rotationRndCoef * UnityEngine.Random.value * 2);
						tmpParticle.size = emitterSystems[n][i].startSize * (1 - arcs[n].emissionOptions[i].randomizationOptions.sizeRndCoef + arcs[n].emissionOptions[i].randomizationOptions.sizeRndCoef * UnityEngine.Random.value) * sizeMultiplier;
						tmpParticle.color = Color.Lerp(emitStartColor,emitEndColor,rand);
						emitterSystems[n][i].Emit(tmpParticle);
						/*emitterSystems[n][i].Emit(Vector3.Lerp(emitPos,GetOldArcPoint(rand,n),UnityEngine.Random.value) + randomVect * radius + spaceShiftVect,
						                          (randomVect * (1f - Mathf.Clamp01(Mathf.Abs(directionCoef))) + emitDir * directionCoef) * emitterSystems[n][i].startSpeed,
						                          emitterSystems[n][i].startSize,
						                          emitterSystems[n][i].startLifetime,
						                          Color.Lerp(emitStartColor,emitEndColor,rand));*/
					}
				}
			}


			//Lights placing
			if (arcs[n].lightsOptions.lights && arcs[n].lightsOptions.priority <= performancePriority)
			{
				for (int i = 0; i < lightsCount[n]; i++)
				{				
					if ((float)(i)/lightsCount[n] <= vertexCnt/vertexCount[n])
					{
						lights[n,i].enabled = true;
						Color mainLightColor;
						if (!arcs[n].colorOptions.onlyStartColor)
							mainLightColor = Color.Lerp(StartColor,EndColor,(float)(i)/(lightsCount[n]-1));
						else
							mainLightColor = StartColor;
						lights[n,i].color = Color.Lerp(mainLightColor,coreColor,coreCoefs[n]/2);
						if (!arcs[n].sizeOptions.onlyStartWidth)
							lights[n,i].intensity = arcs[n].lightsOptions.lightsIntensityMultiplyer * Mathf.Lerp(startWidth,endWidth,(float)i/(segmNums[n]+1));
						else
							lights[n,i].intensity = arcs[n].lightsOptions.lightsIntensityMultiplyer * startWidth;
						lightsTransforms[n,i].position = GetArcPoint((float)(i)/(lightsCount[n]-1),n);
					}
					else
					{
						lights[n,i].enabled = false;
					}
				}
			}
		}
	}
}
