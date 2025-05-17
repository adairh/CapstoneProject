using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject playerPathRendererPrefab;
    private Plane _shapePlane;

    private GameManager gm;

    private RaycastHit[] hits;

    private bool isMovePossible, isPlayerMoving;
    private Camera mainCamera;
    private PlayerPathRenderer pathRenderer;
    private Vector3 position;

    private float upperBound;

    private void Start()
    {
        gm = GameManager.instance;

        _shapePlane = new Plane(Vector3.back, 0.0f);
        mainCamera = Camera.main;
        position = Vector3.one;

        isMovePossible = false;
        isPlayerMoving = false;

        var go = Instantiate(playerPathRendererPrefab, transform.parent);
        pathRenderer = go.GetComponent<PlayerPathRenderer>();

        upperBound = LevelBounds.upperBound + 0.1f;

        GameManager.instance.OnStart += OnGameStart;
        GameManager.instance.OnComplete += OnGameEnd;
        GameManager.instance.OnTimesUp += OnGameEnd;

        enabled = false;
    }

    // Gets inputs from player and executes reactions
    private void Update()
    {
        if (Input.GetMouseButton(0) && !isPlayerMoving && !gm.Quiz.gameObject.activeSelf)
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            float distanceToPlane;

            var nextPosition = Vector3.one;

            if (_shapePlane.Raycast(ray, out distanceToPlane))
                nextPosition = ray.GetPoint(distanceToPlane);

            if (nextPosition.y <= upperBound)
            {
                LevelBounds.KeepInBounds(ref nextPosition);
                position = nextPosition;

                DetectHits();
                CheckIfMoveIsPossible();

                pathRenderer.Draw(transform.position, position, isMovePossible);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (position != Vector3.one && isMovePossible && !isPlayerMoving && !gm.Quiz.gameObject.activeSelf)
            {
                isPlayerMoving = true;

                LeanTween.move(gameObject, position, .25f)
                    .setEaseInExpo()
                    .setOnComplete(() =>
                    {
                        position = Vector3.one;

                        foreach (var hit in hits)
                        {
                            //hit.collider.gameObject.GetComponent<Destructible>().Destroy();
                            var go = hit.collider.gameObject.GetComponent<Destructible>();

                            if (go is Virus)
                            {
                                var v = (Virus)go;
                                v.onHit();
                            }
                            else
                            {
                                go.Destroy();
                            }
                        }

                        isPlayerMoving = false;
                    });

                AudioManager.Play(SFX.Slash);
            }

            pathRenderer.Clear();
        }
    }

    // Sends raycast and checks collision hits
    private void DetectHits()
    {
        var heading = position - transform.position;

        var distance = heading.magnitude;
        var direction = heading / distance;

        hits = Physics.RaycastAll(transform.position, direction, distance);
    }

    // Checks if move to certain point on screen meets requirements
    private void CheckIfMoveIsPossible()
    {
        bool virusFound = false, fatFound = false;
        var lastVirusPosition = Vector3.zero;

        foreach (var hit in hits)
        {
            if (hit.collider.tag == "Virus")
            {
                virusFound = true;
                lastVirusPosition = hit.collider.transform.position;
            }

            if (hit.collider.tag == "Fat")
                fatFound = true;
        }

        isMovePossible = virusFound && !fatFound;

        if (isMovePossible)
        {
            var distance = Vector3.Distance(lastVirusPosition, position);

            if (distance < 0.5f)
                isMovePossible = false;
        }
    }

    // Enables game controller when countdown has finished
    private void OnGameStart()
    {
        enabled = true;
    }

    // Disables game controller when one of endings occured
    private void OnGameEnd()
    {
        pathRenderer.Clear();
        enabled = false;
    }
}