using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StrangeSpace
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField]
        private GameObject _spinner;

        [SerializeField]
        private float _rotationSpeed = -100;

        private float _timeToShowSpinner = 0;
        private bool _showingSpinner;
        private Vector3 _spinnerAngle;

        private void Start()
        {
            _spinnerAngle = _spinner.transform.eulerAngles;
        }

        private void Update()
        {
            if (_timeToShowSpinner > 0)
            {
                _timeToShowSpinner -= Time.deltaTime;
                if (_timeToShowSpinner <= 0)
                {
                    ShowSpinner(true);
                }
            }

            if (_showingSpinner)
            {
                _spinnerAngle.z += Time.deltaTime * _rotationSpeed;
                _spinner.transform.eulerAngles = _spinnerAngle;
            }
        }

        /// <summary>
        /// Show screen WITH the spinner
        /// </summary>
        /// <param name="spinnerDelay"> Time delay to show the spinner </param>
        public void ShowWithSpinner(float spinnerDelay = 0)
        {
            Show();

            if (spinnerDelay > 0)
            {
                _timeToShowSpinner = spinnerDelay;
            }
            else
            {
                ShowSpinner(true);
            }
        }

        /// <summary>
        /// Show screen WITHOUT the spinner
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            ShowSpinner(false);
            _timeToShowSpinner = 0;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _timeToShowSpinner = 0;
        }

        private void ShowSpinner(bool show)
        {
            _showingSpinner = show;
            _spinner.SetActive(show);
        }
    }

    /*
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField]
        private GameObject _spinner;

        [SerializeField]
        private float _rotationSpeed = -100;

        private float _timeToShowSpinner = 0;
        private bool _showingSpinner;
        private Vector3 _spinnerAngle;

        private void Start()
        {
            _spinnerAngle = _spinner.transform.eulerAngles;
        }

        private void Update()
        {
            if (_timeToShowSpinner > 0)
            {
                _timeToShowSpinner -= Time.deltaTime;
                if (_timeToShowSpinner <= 0)
                {
                    ShowSpinner(true);
                }
            }

            if (_showingSpinner)
            {
                _spinnerAngle.z += Time.deltaTime * _rotationSpeed;
                _spinner.transform.eulerAngles = _spinnerAngle;
            }
        }

        /// <summary>
        /// Show screen WITH the spinner
        /// </summary>
        /// <param name="spinnerDelay"> Time delay to show the spinner </param>
        public void ShowWithSpinner(float spinnerDelay = 0)
        {
            Show();

            if (spinnerDelay > 0)
            {
                _timeToShowSpinner = spinnerDelay;
            }
            else
            {
                ShowSpinner(true);
            }
        }

        /// <summary>
        /// Show screen WITHOUT the spinner
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            ShowSpinner(false);
            _timeToShowSpinner = 0;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _timeToShowSpinner = 0;
        }

        private void ShowSpinner(bool show)
        {
            _showingSpinner = show;
            _spinner.SetActive(show);
        }
    }*/
}
