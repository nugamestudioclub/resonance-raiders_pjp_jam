using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveCollider : MonoBehaviour
{
    public enum WaveType {DAMAGE,DISRUPTION}


    public float speed = 1f;
    [HideInInspector]
    public WaveCollider next;
    public GameObject lineRenderer;
    public float splitThreshold = 1f;
    public Vector3 velocity;
    private Rigidbody rb;
    
    public float timeLeft = 20f;
    private float totalTime;
    public float velocityReductionOnHit = 1f;
    public Color tint;
    public int id = 0;
    public float damage = 0f;
    public float disruption = 0f;
    public WaveType type;
    public PlayerValues playerValues;

    private Vector3 initPosition;
    private Vector3 initLPosition;

    private MeshRenderer mRenderer;

    private float timeSinceLastBounce = 0f;
    private void Start()
    {
        totalTime = timeLeft;
        lineRenderer.GetComponent<LineRenderer>().enabled = false;
        //velocity = transform.forward;
        rb = GetComponent<Rigidbody>();
        initPosition = rb.position;
        initLPosition = transform.localPosition;
        mRenderer = GetComponentInChildren<MeshRenderer>();
        
        //rb.AddForce(transform.right * speed, ForceMode.Impulse);

        //rb.velocity = transform.right * speed * Time.deltaTime;
    }
    public void ResetPos()
    {
        this.gameObject.SetActive(true);
        this.timeLeft = totalTime;
        rb.position = initPosition;
        transform.localPosition = initLPosition;
    }
    private void Update()
    {
        tint = ColorMixing.MixColors(playerValues.destructionColor,playerValues.distruptionColor,damage,disruption);
        mRenderer.materials[0].SetColor("_BaseColor", new Color(tint.r, tint.g, tint.b, timeLeft / totalTime));
        rb.position += velocity * Time.deltaTime * speed;
        timeSinceLastBounce -= Time.deltaTime;
        //transform.localPosition += velocity * Time.deltaTime * speed;
        if (next != null&&Vector3.Distance(this.transform.position,next.transform.position)<splitThreshold)
        {
            lineRenderer.GetComponent<LineRenderer>().enabled = true;
            LineRenderer rend = lineRenderer.GetComponent<LineRenderer>();
            rend.positionCount = 2;
            rend.SetPosition(0, Vector3.zero);
            rend.SetPosition(1, next.transform.position - transform.position);
            //Set Color opacity
            rend.startColor = new Color(tint.r,tint.g, tint.b, timeLeft / totalTime);
            //Add renderer here
            mRenderer.materials[0].SetColor("_BaseColor", new Color(tint.r, tint.g, tint.b, timeLeft / totalTime));
            rend.endColor = rend.startColor;
        }
        else
        {
            lineRenderer.GetComponent<LineRenderer>().enabled = false;
        }
        
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0)
        {
            //this.gameObject.SetActive(false);
            this.gameObject.SetActive(false);
            //Destroy(this.gameObject);
        }

        if (transform.position.x > playerValues.xBoundary[1]||transform.position.x < playerValues.xBoundary[0] || transform.position.z > playerValues.yBoundary[1] || transform.position.z < playerValues.yBoundary[0])
        {
            this.gameObject.SetActive(false);
        }
    }
    public static class ColorMixing
    {
        public static Color MixColors(Color color1, Color color2, float value1, float value2)
        {
            // Normalize the values to be in the range [0, 1]
            float totalValue = value1 + value2;
            if (totalValue == 0)
                return Color.black;

            float normalizedValue1 = value1 / totalValue;
            float normalizedValue2 = value2 / totalValue;

            // Mix the colors based on the normalized values
            float r = color1.r * normalizedValue1 + color2.r * normalizedValue2;
            float g = color1.g * normalizedValue1 + color2.g * normalizedValue2;
            float b = color1.b * normalizedValue1 + color2.b * normalizedValue2;
            float a = color1.a * normalizedValue1 + color2.a * normalizedValue2;

            return new Color(r, g, b, a);
        }
    }
    public class VectorUtils : MonoBehaviour
    {
        public static float AngleBetweenVectors(Vector3 vector1, Vector3 vector2)
        {
            float dotProduct = Vector3.Dot(vector1.normalized, vector2.normalized);
            float angleInRadians = Mathf.Acos(dotProduct);
            float angleInDegrees = Mathf.Rad2Deg * angleInRadians;
            return angleInDegrees;
        }
        public static Vector2 RotateVector(Vector2 vector, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cosAngle = Mathf.Cos(radians);
            float sinAngle = Mathf.Sin(radians);
            float newX = vector.x * cosAngle - vector.y * sinAngle;
            float newY = vector.x * sinAngle + vector.y * cosAngle;
            return new Vector2(newX, newY);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<WaveCollider>() != null)
        {
            WaveCollider col = other.GetComponent<WaveCollider>();
            if (col.id != id)
            {
                tint = Color.red;
                switch (col.type)
                {
                    case WaveType.DISRUPTION:
                        disruption += .5f;
                        break;
                    case WaveType.DAMAGE:
                        damage += .5f;
                        break;
                }
            }
        }
        
        if(other.GetComponent<Enemy>()!= null)
        {
            Enemy e = other.GetComponent<Enemy>();
            
            EnemyEffect effect = new Damage(Mathf.RoundToInt(damage));
            e.AddEffect(effect);
            effect = new ChangeSpeed(Mathf.Max(1-disruption,0), playerValues.disruptionDuration);
            e.AddEffect(effect);
            this.gameObject.SetActive(false);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.GetComponent<DeflectorComponent>() == null)
        {
            return;
        }
        Physics.IgnoreCollision(collision.collider, this.GetComponent<SphereCollider>(), true);
        

        ContactPoint[] contacts = collision.contacts;
        if (contacts.Length > 0)
        {
            Vector3 averageNormal = Vector3.zero;
            Dictionary<Vector3,int> normals = new Dictionary<Vector3, int>();
            foreach (ContactPoint contact in contacts)
            {
                averageNormal += contact.normal;
                if (normals.ContainsKey(contact.normal))
                    normals[contact.normal] += 1;
                else
                    normals[contact.normal] = 1;
                
            }
            
            
            
            averageNormal /= contacts.Length;

            // Calculate the tangent vector to the collision surface
            Vector3 refVector = Vector3.up;
            if (Vector3.Angle(averageNormal, Vector3.up) < 0.01f) // checking if averageNormal is close to Vector3.up
            {
                refVector = Vector3.right;
            }

           

            // Create two vectors perpendicular to averageNormal using Gram-Schmidt process
            Vector3 tangent = Vector3.Cross(refVector, averageNormal).normalized;
             
            //Vector3 bitangent = Vector3.Cross(averageNormal, tangent).normalized;


            // Use the tangent vector for further calculations or processing

            tangent = Quaternion.Euler(0, -90, 0) * tangent;
            velocity += tangent*2;
            velocity *= velocityReductionOnHit;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        Physics.IgnoreCollision(collision.collider, this.GetComponent<SphereCollider>(), false);
    }
}
