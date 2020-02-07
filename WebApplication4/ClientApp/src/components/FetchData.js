import React, { Component } from 'react';
import Select from "react-select";

const options = [
    { value: 'hepsi', label: 'Hepsi' },
    { value: 'alt', label: 'Ortalama Altında Kalanlar' },
    { value: 'ust', label: 'Ortalama Üstünde Kalanlar' },
];



export class FetchData extends Component {
    static displayName = FetchData.name;



    constructor(props) {
        super(props);
        this.state = { forecasts: [], loading: true, countries: [] };
    }

    handleChange = (event) => {
        this.setState({ [event.label]: event.value });
        this.innerHTML = event.label;
        var values = document.getElementById('manager').options[document.getElementById('manager').selectedIndex].text;
        var manager = "";
        if (values != "") {
            manager = values;
        }
        console.log(values);
        this.populateWeatherDataFilter(event.value, manager);
    };
    handleChangeManager = (event) => {
        //this.setState({ [event.label]: event.value });
        //this.innerHTML = event.label;
        var values = document.getElementById('manager').options[document.getElementById('manager').selectedIndex].text;
        console.log(document.getElementById('manager').options[document.getElementById('manager').selectedIndex].text);
        var ortalama = "";
        if (values.innerText == "Ortalama Altında Kalanlar") {
            ortalama = "alt"
        }
        else if (values.innerText == "Ortalama Üstünde Kalanlar") {
            ortalama = "üst"
        }
        console.log(event.target.value);


        this.populateWeatherDataFilter(ortalama, event.target.value);
        //this.populateWeatherDataFilter(event.value);
    };
    componentDidMount() {
        this.populateWeatherData();

    }




    static renderForecastsTable(forecasts) {
        return (
            <table id="table1" class='table table-hover"' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Ad Soyad</th>
                        <th>Tarih  Aralığı</th>
                        <th>Çalışma Gün Sayısı</th>
                        <th>Çalışma Saati(Dakika)</th>
                        <th>Ortalama Çalışılması Gereken Saat (Dakika)</th>
                    </tr>
                </thead>
                <tbody>
                    {forecasts.raporList.map(forecast =>
                        <tr rows={forecast.users.userId} class={rowClassNameFormat(forecast.ortalamaninAltinda)}>
                            <td>{forecast.users.adSoyad}</td>
                            <td>{forecast.tarihAraligi}</td>
                            <td>{forecast.toplamCalismaGunSayisi}</td>
                            <td>{forecast.toplamCalismaSaati} Saat -( <small class="text-muted"> {forecast.toplamCalismaDakika} Dakika</small> ) </td>
                            <td>{forecast.ortalamaCalisilmasiGerekenSaat} Saat -( <small class="text-muted"> {forecast.ortalamaCalisilmasiGerekenDakika} Dakika</small> ) </td>
                        </tr>
                    )}
                </tbody>
            </table>
        );
    }

    render() {
        const { data } = this.state;

        const { countries } = this.state;

        let contents = this.state.loading
            ? <p><em>Yükleniyor...</em></p>
            : FetchData.renderForecastsTable(this.state.forecasts);

        let countriesList = countries.length > 0
            ? countries.map((item, i) => {
                return (
                    <option key={i} value={item.key}>{item.managerName}</option>
                )
            }, this)
            : <p><em>Yükleniyor...</em></p>;

        return (
            <div class="container">
                <h3 id="tabelLabel">Puantaj Raporu </h3>

                <div class="row">
                    <div class="col-md-3">
                        Yönetici:
                    </div>
                    <div class="col-md-5">
                        <select id="manager" onChange={this.handleChangeManager.bind(this)} value={this.state.value} >
                            <option value="" >Yönetici Seçiniz. </option>
                            {countriesList}
                        </select>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-3">
                        Ortalama:
                    </div>

                    <div class="col-md-5">
                        <Select
                            id="ortalamaselect"
                            onChange={this.handleChange}
                            options={options}
                        />
                    </div>
                </div>

                <div class="row">
                    <div class="col-12">

                        {contents}
                    </div>

                </div>
            </div >


        );
    }

    async populateWeatherData() {
        const response = await fetch('weatherforecast');
        const data = await response.json();
        this.setState({ forecasts: data, loading: false });
        console.log(data);
        this.setState({ countries: data.managerList });
    }

    async populateWeatherDataFilter(ortalama,manager) {
        this.setState({ forecasts: [], loading: true });
        const response = await fetch('weatherforecast?ortalama=' + ortalama+'&manager='+manager);
        const data = await response.json();
        console.log(data);
        this.setState({ forecasts: data, loading: false });

    }


}

function rowClassNameFormat(value) {

    if (value == false) {
        return "table-success";
    }
    else {
        return "table-danger";
    }

}
